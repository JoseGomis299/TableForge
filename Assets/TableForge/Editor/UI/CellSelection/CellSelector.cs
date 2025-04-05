using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellSelector : ICellSelector
    {
        public event Action OnSelectionChanged;
        
        private readonly TableControl _tableControl;
        private readonly HashSet<Cell> _selectedCells = new HashSet<Cell>();
        private HashSet<Cell> _cellsToDeselect = new HashSet<Cell>();
        private readonly HashSet<CellAnchor> _selectedAnchors = new HashSet<CellAnchor>();
        private Vector3 _lastMousePosition;
        
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;
        public HashSet<Cell> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }
        
        public Cell FirstSelectedCell { get; set; }
        public Cell FocusedCell { get; set; }

        public TableControl TableControl => _tableControl;

        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            _tableControl.ScrollView.contentContainer.RegisterCallback<PointerDownEvent>(PreselectCells, TrickleDown.TrickleDown);
            _tableControl.ScrollView.contentContainer.RegisterCallback<PointerDownEvent>(ConfirmSelection, TrickleDown.NoTrickleDown);
            _tableControl.ScrollView.contentContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private bool IsValidClick(PointerDownEvent evt)
        {
            return evt.button == 0 &&
                   !_tableControl.VerticalResizer.IsResizing &&
                   !_tableControl.HorizontalResizer.IsResizing &&
                   !_tableControl.VerticalResizer.IsResizingArea(evt.position, out _) &&
                   !_tableControl.HorizontalResizer.IsResizingArea(evt.position, out _);
        }

        private void ConfirmSelection(PointerDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            ConfirmSelection();
            evt.StopImmediatePropagation();
        }

        private void ConfirmSelection()
        {
            // Deselect previously selected headers.
            _selectedAnchors.Clear();

            // For each selected cell, if it is not marked for deselection, mark it and its headers as selected.
            foreach (var cell in _selectedCells)
            {
                if (_cellsToDeselect.Contains(cell))
                    continue;
                
                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if(cellControl != null) cellControl.IsSelected = true;
                
                _selectedAnchors.Add(cell.Row);
                _selectedAnchors.Add(cell.Column);
            }

            // Deselect cells marked for removal.
            foreach (var cell in _cellsToDeselect)
            {
                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if(cellControl != null) cellControl.IsSelected = false;
                
                _selectedCells.Remove(cell);
            }
            _cellsToDeselect.Clear();
            
            OnSelectionChanged?.Invoke();
        }

        private void PreselectCells(PointerDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            List<Cell> cellsAtPosition = GetCellsAtPosition(evt.position);
            _lastMousePosition = evt.position;

            // Use the proper selection strategy based on modifier keys.
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy(evt);
            Cell lastSelectedCell = strategy.Preselect(this, cellsAtPosition);
            
            // If the last selected cell is a reference cell, immediately confirm the selection.
            if (lastSelectedCell is ReferenceCell)
            {
                ConfirmSelection();
            }
        }

        private List<Cell> GetCellsAtPosition(Vector3 position)
        {
            var selectedCells = new List<Cell>();
            GetCellsAtPosition(position, _tableControl, selectedCells);
            return selectedCells;
        }

        private void GetCellsAtPosition(Vector3 position, TableControl tableControl, List<Cell> outputCells)
        {
            if(!tableControl.ScrollView.contentContainer.worldBound.Contains(position))
                return;
            
            if (tableControl.CornerContainer.worldBound.Contains(position))
            {
                foreach (var row in tableControl.TableData.Rows)
                    outputCells.AddRange(row.Value.Cells.Values);
                
                return;
            }
            
            var headers = CellLocator.GetHeadersAtPosition(tableControl, position);
            if (headers.row == null && headers.column == null)
                return;

            if (headers.row == null)
            {
                outputCells.AddRange(CellLocator.GetCellsAtColumn(tableControl, headers.column.Id));
                return;
            }

            if (headers.column == null)
            {
                outputCells.AddRange(CellLocator.GetCellsAtRow(tableControl, headers.row.Id));
                return;
            }

            var cell = CellLocator.GetCell(tableControl, headers.row.Id, headers.column.Id);
            int count = outputCells.Count, prevCount = outputCells.Count;
            
            if(cell is SubTableCell subTableCell && _selectedCells.Contains(subTableCell))
            {
                TableControl subTable = (CellControlFactory.GetCellControlFromId(subTableCell.Id) as SubTableCellControl)?.SubTableControl;
                if(subTable != null)
                {
                    GetCellsAtPosition(position, subTable, outputCells);
                    count = outputCells.Count;
                }
            }
            
            if((cell != null && cell is not SubTableCell) || count == prevCount)
                outputCells.Add(cell);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons != 1 ||
                _tableControl.VerticalResizer.IsResizing ||
                _tableControl.HorizontalResizer.IsResizing)
            {
                return;
            }

            // Only proceed if the mouse has moved a minimum distance.
            float distance = Vector3.Distance(evt.mousePosition, _lastMousePosition);
            if (distance < UiConstants.MoveSelectionStep)
                return;

            _lastMousePosition = evt.mousePosition;
            Cell selectedCell = GetCellsAtPosition(_lastMousePosition).FirstOrDefault();

            if (selectedCell == null || FirstSelectedCell == null)
                return;

            // Determine the range from the first selected cell to the cell under the cursor.
            Cell firstCell = FirstSelectedCell;
            Cell lastCell = selectedCell;
            var cells = CellLocator.GetCellRange(firstCell, lastCell, _tableControl);

            // Mark all cells currently selected for potential deselection.
            _cellsToDeselect = new HashSet<Cell>(_selectedCells);
            foreach (var cell in cells)
            {
                _selectedCells.Add(cell);
                _cellsToDeselect.Remove(cell);
            }

            ConfirmSelection();
        }

        private void SelectAll(Table table)
        {
            foreach (var row in table.Rows.Values)
            {
                foreach (var cell in row.Cells.Values)
                {
                    _selectedCells.Add(cell);
                    
                    if(cell is SubTableCell subCell &&
                       TableControl.Metadata.IsTableExpanded(subCell.Id))
                        SelectAll(subCell.SubTable);
                }
            }
        }

        public void ClearSelection()
        {
            _selectedAnchors.Clear();
            _selectedCells.Clear();
            _cellsToDeselect.Clear();
            
            OnSelectionChanged?.Invoke();
        }
    }
}
