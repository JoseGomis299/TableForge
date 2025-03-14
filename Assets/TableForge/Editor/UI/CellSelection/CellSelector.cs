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
        private readonly HashSet<HeaderControl> _selectedHeaders = new HashSet<HeaderControl>();
        private Vector3 _lastMousePosition;
        
        public HashSet<Cell> SelectedCells => _selectedCells;
        public HashSet<Cell> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }
        public Cell FirstSelectedCell { get; set; }

        public TableControl TableControl => _tableControl;

        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            _tableControl.RegisterCallback<PointerDownEvent>(PreselectCells, TrickleDown.TrickleDown);
            _tableControl.RegisterCallback<PointerDownEvent>(ConfirmSelection, TrickleDown.NoTrickleDown);
            _tableControl.RegisterCallback<MouseMoveEvent>(OnMouseMove);
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
            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = false;
            }
            _selectedHeaders.Clear();

            // For each selected cell, if it is not marked for deselection, mark it and its headers as selected.
            foreach (var cell in _selectedCells)
            {
                if (_cellsToDeselect.Contains(cell))
                    continue;

                int columnId = _tableControl.Inverted ? cell.Row.Id : cell.Column.Id;
                int rowId = _tableControl.Inverted ? cell.Column.Id : cell.Row.Id;
                _selectedHeaders.Add(_tableControl.ColumnHeaders[columnId]);
                _selectedHeaders.Add(_tableControl.RowHeaders[rowId]);
            }

            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = true;
            }

            // Deselect cells marked for removal.
            foreach (var cell in _cellsToDeselect)
            {
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
            Cell lastSelectedCell = strategy.Preselect(this, evt, cellsAtPosition);

            if(evt.clickCount == 2)
            {
                foreach (var selectedCell in _selectedCells)
                {
                   if(selectedCell is SubTableCell subCell)
                       SelectAll(subCell.Table);
                }
            }
            
            // If the last selected cell is a reference cell, immediately confirm the selection.
            else if (lastSelectedCell is ReferenceCellControl)
            {
                ConfirmSelection();
            }
        }

        private List<Cell> GetCellsAtPosition(Vector3 position)
        {
            var selectedCells = new List<Cell>();
            if (_tableControl.CornerContainer.worldBound.Contains(position))
            {
                foreach (var row in _tableControl.TableData.Rows)
                    selectedCells.AddRange(row.Value.Cells.Values);
                
                return selectedCells;
            }
            
            var headers = CellLocator.GetHeadersAtPosition(_tableControl, position);

            if (headers.row == null && headers.column == null)
                return selectedCells;

            if (headers.row == null)
                return CellLocator.GetCellsAtColumn(_tableControl, headers.column.Id);

            if (headers.column == null)
                return CellLocator.GetCellsAtRow(_tableControl, headers.row.Id);

            var cell = CellLocator.GetCell(_tableControl, headers.row.Id, headers.column.Id);
            if (cell != null)
                selectedCells.Add(cell);

            return selectedCells;
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

            var headers = CellLocator.GetHeadersAtPosition(_tableControl, evt.mousePosition);
            Cell selectedCell = null;
            if (headers.row != null && headers.column != null)
            {
                selectedCell = CellLocator.GetCell(_tableControl, headers.row.Id, headers.column.Id);
            }

            if (selectedCell == null || FirstSelectedCell == null)
                return;

            // Determine the range from the first selected cell to the cell under the cursor.
            Cell firstCell = FirstSelectedCell;
            Cell lastCell = selectedCell;
            var firstRow = _tableControl.GetCellRow(firstCell);
            var lastRow = _tableControl.GetCellRow(lastCell);
            var firstColumn = _tableControl.GetCellColumn(firstCell);
            var lastColumn = _tableControl.GetCellColumn(lastCell);
            var cells = CellLocator.GetCellRange(_tableControl, firstRow.Id, firstColumn.Id, lastRow.Id, lastColumn.Id);

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
            _selectedCells.Clear();
            foreach (var row in table.Rows.Values)
            {
                foreach (var cell in row.Cells.Values)
                {
                    _selectedCells.Add(cell);
                    
                    if(cell is SubTableCell subCell)
                        SelectAll(subCell.SubTable);
                }
            }
            
            ConfirmSelection();
        }

        public void ClearSelection()
        {
            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = false;
            }
            
            _selectedHeaders.Clear();
            _selectedCells.Clear();
            _cellsToDeselect.Clear();
            
            OnSelectionChanged?.Invoke();
        }
    }
}
