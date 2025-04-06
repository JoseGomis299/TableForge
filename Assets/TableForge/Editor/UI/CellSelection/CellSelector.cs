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
        private Cell _firstSelectedCell;
        private List<Cell> _orderedSelectedCells = new List<Cell>();
        
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;
        public HashSet<Cell> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }
        
        public bool SelectionEnabled {get; set;}

        public Cell FirstSelectedCell
        {
            get => _firstSelectedCell;
            set
            {
                if (_firstSelectedCell == value)
                {
                    if (value == null) return;
                    _selectedCells.Add(value);
                    _cellsToDeselect.Remove(value);
                    return;
                }
                Cell previousFirstSelectedCell = _firstSelectedCell;
                _firstSelectedCell = value;
                
                if(previousFirstSelectedCell != null)
                {
                    CellControl previousCellControl = CellControlFactory.GetCellControlFromId(previousFirstSelectedCell.Id);
                    if(previousCellControl != null)
                    {
                        previousCellControl.focusable = false;
                        previousCellControl.RemoveFromClassList(USSClasses.FirstSelected);
                    }
                }

                if (value == null) return;
                CellControl cellControl = CellControlFactory.GetCellControlFromId(value.Id);
                if(cellControl != null)
                {
                    cellControl.focusable = true;
                    cellControl?.Focus();
                    cellControl.AddToClassList(USSClasses.FirstSelected);

                    foreach (var ancestor in cellControl.GetAncestors(true))
                    {
                        
                    }
                }
                
               
                
                _selectedCells.Add(value);
                _cellsToDeselect.Remove(value);
            }
        }

        public TableControl TableControl => _tableControl;

        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            _tableControl.ScrollView.contentContainer.RegisterCallback<MouseDownEvent>(PreselectCells, TrickleDown.TrickleDown);
            _tableControl.ScrollView.contentContainer.RegisterCallback<MouseDownEvent>(ConfirmSelection, TrickleDown.NoTrickleDown);
            _tableControl.ScrollView.contentContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _tableControl.Root.RegisterCallback<KeyDownEvent>(HandleKeyDown);
            
            SelectionEnabled = true;
        }

        private bool IsValidClick(IMouseEvent evt)
        {
            return evt.button == 0 
                   && SelectionEnabled;
        }
        
        private void HandleKeyDown(KeyDownEvent evt)
        {
            if(!SelectionEnabled) return;
            
            Vector2 direction = Vector2.zero;
            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    direction = Vector2.up;
                    break;
                case KeyCode.DownArrow:
                    direction = Vector2.down;
                    break;
                case KeyCode.LeftArrow:
                    direction = Vector2.left;
                    break;
                case KeyCode.RightArrow:
                    direction = Vector2.right;
                    break;
            }
            
            if (direction != Vector2.zero)
            {
                if (FirstSelectedCell == null)
                {
                    if (TableControl.TableData.GetCell("A1") is not { } cell) return;
                
                    FirstSelectedCell = cell;
                    ConfirmSelection();
                    return;
                }
                
                _cellsToDeselect = new HashSet<Cell>(_selectedCells);
                
                Cell firstCell = FirstSelectedCell;
                Vector2 wrappingMinBounds = new Vector2(1, 1);
                Vector2 wrappingMaxBounds = new Vector2(firstCell.Table.Columns.Count, firstCell.Table.Rows.Count);
                FirstSelectedCell = CellLocator.GetContiguousCell(firstCell, direction, wrappingMinBounds, wrappingMaxBounds);

                foreach (var ancestor in FirstSelectedCell.GetAncestors())
                {
                    _cellsToDeselect.Remove(ancestor);
                }
                
                ConfirmSelection();
            }
            
            else if(evt.keyCode is KeyCode.KeypadEnter or KeyCode.Return)
            {
                if (FirstSelectedCell is SubTableCell subTableCell)
                {
                    CellControl cellControl = CellControlFactory.GetCellControlFromId(FirstSelectedCell.Id);
                    if (cellControl is ExpandableSubTableCellControl expandableSubTable)
                    {
                        if (!expandableSubTable.IsFoldoutOpen)
                        {
                            expandableSubTable.OpenFoldout();
                        }
                        
                        Cell first = subTableCell.SubTable.GetCell("A1");
                        if (first != null)
                        {
                            FirstSelectedCell = first;
                            ConfirmSelection();
                        }
                    }
                }
                else if (FirstSelectedCell is {})
                {
                    CellControl cellControl = CellControlFactory.GetCellControlFromId(FirstSelectedCell.Id);
                    if (cellControl is ISimpleCellControl simpleCellControl && !simpleCellControl.IsFieldFocused())
                    {
                        simpleCellControl.FocusField();
                    }
                }
            }

            else if (evt.keyCode is KeyCode.LeftShift or KeyCode.RightShift or KeyCode.Backspace && FirstSelectedCell.Table.IsSubTable)
            {
                FirstSelectedCell = FirstSelectedCell.Table.ParentCell;
                foreach (var descendants in FirstSelectedCell.GetDescendants())
                {
                    _cellsToDeselect.Add(descendants);
                }
                
                ConfirmSelection();
            }

            else if(FirstSelectedCell is {} && evt.character is >= '!' and <= '~')
            {
                CellControl cellControl = CellControlFactory.GetCellControlFromId(FirstSelectedCell.Id);
                if (cellControl is ITextBasedCellControl textBasedCellControl && !textBasedCellControl.IsFieldFocused())
                {
                    textBasedCellControl.SetValue(evt.character.ToString(), true);
                    
                }
            }

            else if(evt.keyCode == KeyCode.Tab)
            {
                if (FirstSelectedCell == null)
                {
                    if (TableControl.TableData.GetCell("A1") is not { } cell) return;
                
                    FirstSelectedCell = cell;
                    ConfirmSelection();
                    return;
                }
                
                CellControl cellControl = CellControlFactory.GetCellControlFromId(FirstSelectedCell.Id);
                if (cellControl is ISimpleCellControl simpleCellControl && simpleCellControl.IsFieldFocused()) return;

                var ancestors = FirstSelectedCell.GetAncestors();
                if (_selectedCells.Count(x => !ancestors.Contains(x)) <= 1)
                {
                    _cellsToDeselect = new HashSet<Cell>(_selectedCells);

                    Vector2Int wrappingMinBounds = new Vector2Int(1, 1);
                    Vector2Int wrappingMaxBounds = new Vector2Int(FirstSelectedCell.Table.Columns.Count, FirstSelectedCell.Table.Rows.Count);
                    
                    FirstSelectedCell = CellLocator.GetContiguousCell(FirstSelectedCell, Vector2.right, wrappingMinBounds, wrappingMaxBounds);
                    
                    foreach (var ancestor in FirstSelectedCell.GetAncestors())
                    {
                        _cellsToDeselect.Remove(ancestor);
                    }

                    ConfirmSelection();
                }
                else
                {
                    int index = _orderedSelectedCells.IndexOf(FirstSelectedCell);
                    index = (index + 1) % _orderedSelectedCells.Count;
                    
                    FirstSelectedCell = _orderedSelectedCells[index];
                }
            }
            
            evt.StopPropagation();
            
        }

        private void ConfirmSelection(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            ConfirmSelection();
            evt.StopPropagation();
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
            
            _orderedSelectedCells = _selectedCells
                .OrderBy(cell => cell.GetHighestAncestor().Row.Position)
                .ThenBy(cell => cell.GetHighestAncestor().Column.Position)
                .ThenBy(cell => cell.GetLevel())
                .ThenBy(cell => cell.Row.Position)
                .ThenBy(cell => cell.Column.Position)
                .ToList();
            
            OnSelectionChanged?.Invoke();
        }

        private void PreselectCells(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            List<Cell> cellsAtPosition = GetCellsAtPosition(evt.mousePosition);
            _lastMousePosition = evt.mousePosition;

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
            if (evt.pressedButtons != 1 || !IsValidClick(evt))
                return;

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
    }
}
