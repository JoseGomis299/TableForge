using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellSelector
    {
        private TableControl _tableControl;
        public HashSet<CellControl> _selectedCells = new HashSet<CellControl>();
        public HashSet<CellControl> _cellsToDeselect = new HashSet<CellControl>();
        private HashSet<HeaderControl> _selectedHeaders = new HashSet<HeaderControl>();
        public CellControl _firstSelectedCell;
        private Vector3 _lastMousePosition;
        
        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            _tableControl.RegisterCallback<MouseDownEvent>(PreselectCells, TrickleDown.TrickleDown);
            _tableControl.RegisterCallback<MouseDownEvent>(ConfirmSelection, TrickleDown.NoTrickleDown);
            _tableControl.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void ConfirmSelection(IMouseEvent evt)
        {
            if (evt.button != 0) return;
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = false;
            }
            _selectedHeaders.Clear();
            
            foreach (var selectedCell in _selectedCells)
            {
                if(_cellsToDeselect.Contains(selectedCell)) continue;
                
                selectedCell.IsSelected = true;
                _selectedHeaders.Add(_tableControl.ColumnHeaders[selectedCell.Cell.Column.Id]);
                _selectedHeaders.Add(_tableControl.RowHeaders[selectedCell.Cell.Row.Id]);
            }
            
            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = true;
            }
            
            foreach (var cell in _cellsToDeselect)
            {
                DeselectCell(cell);
            }
            _cellsToDeselect.Clear();
        }

        private void PreselectCells(MouseDownEvent evt)
        {
            if(evt.button != 0) return;
            
            List<CellControl> cellsAtPosition = GetCellsAtPosition(evt.mousePosition);
            _lastMousePosition = evt.mousePosition;

            // Choose a selection strategy based on modifier keys.
            ISelectionStrategy strategy = GetSelectionStrategy(evt);
            CellControl lastSelectedCell = strategy.Preselect(this, evt, cellsAtPosition);

            //If the last selected cell is a reference cell, we want to confirm the selection
            //We need to do this because the ObjectField interrupts the mouse down bubble up event (which is used to confirm the selection)
            if(lastSelectedCell is ReferenceCellControl)
            {
                ConfirmSelection();
            }
        }

        private ISelectionStrategy GetSelectionStrategy(MouseDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                return new CtrlSelectionStrategy();
            }

            if (evt.shiftKey)
            {
                return new ShiftSelectionStrategy();
            }

            return new NormalSelectionStrategy();
        }

        private List<CellControl> GetCellsAtPosition(Vector3 position)
        {
            var headers = CellLocator.GetHeadersAtPosition(_tableControl, position);
            List<CellControl> selectedCells = new List<CellControl>();

            switch (headers.row)
            {
                case null when headers.column == null:
                    break;
                case null:
                    selectedCells = CellLocator.GetCellsAtColumn(_tableControl, headers.column.Id);
                    break;
                default:
                    {
                        if (headers.column == null)
                        {
                            selectedCells = CellLocator.GetCellsAtRow(_tableControl, headers.row.Id);
                        }
                        else
                        {
                            var cell = CellLocator.GetCell(_tableControl, headers.row.Id, headers.column.Id);
                            if (cell != null)
                            {
                                selectedCells.Add(cell);
                            }
                        }
                        break;
                    }
            }

            return selectedCells;
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if(evt.pressedButtons != 1) return;
            
            // If the mouse has not moved far enough, do not do anything.
            float distance = Vector3.Distance(evt.mousePosition, _lastMousePosition);
            if (distance < UiContants.MoveSelectionStep) return;
            
            _lastMousePosition = evt.mousePosition;

            var headers = CellLocator.GetHeadersAtPosition(_tableControl, evt.mousePosition);
            CellControl selectedCell = null;
            if(headers.row != null && headers.column != null)
            {
                selectedCell = CellLocator.GetCell(_tableControl, headers.row.Id, headers.column.Id);
            }
            
            if(selectedCell == null || _firstSelectedCell == null) return;
            
            var firstCell = _firstSelectedCell;
            var lastCell = selectedCell;
            var firstRow = firstCell.Cell.Row;
            var lastRow = lastCell.Cell.Row;
            var firstColumn = firstCell.Cell.Column;
            var lastColumn = lastCell.Cell.Column;
            var cells = CellLocator.GetCellRange(_tableControl, firstRow.Id, firstColumn.Id, lastRow.Id, lastColumn.Id);

            ClearSelection();
            foreach (var cell in cells)
            {
                SelectCell(cell);
            }
        }

        public void SelectCell(CellControl cellControl)
        {
            _selectedCells.Add(cellControl);
            cellControl.IsSelected = true;
        }
        
        public void DeselectCell(CellControl cellControl)
        {
            _selectedCells.Remove(cellControl);
            cellControl.IsSelected = false;
        }
        
        public void ClearSelection()
        {
            foreach (var cell in _selectedCells)
            {
                cell.IsSelected = false;
            }
            _selectedCells.Clear();
            _cellsToDeselect.Clear();
        }
        
        
        /// <summary>
        /// Interface for the selection strategy.
        /// </summary>
        private interface ISelectionStrategy
        {
            /// <summary>
            /// Executes the preselection using the given mouse event and list of cells at the mouse position.
            /// Returns the "last selected cell" (which may be used for further actions).
            /// </summary>
            CellControl Preselect(CellSelector selector, MouseDownEvent evt, List<CellControl> cellsAtPosition);
        }

        /// <summary>
        /// Implements the behavior when CTRL is held:
        /// - If a cell is already selected, it is marked for deselection.
        /// - Otherwise, the cell is added.
        /// </summary>
        private class CtrlSelectionStrategy : ISelectionStrategy
        {
            public CellControl Preselect(CellSelector selector, MouseDownEvent evt, List<CellControl> cellsAtPosition)
            {
                CellControl lastSelectedCell = null;
                foreach (var cell in cellsAtPosition)
                {
                    if (selector._selectedCells.Contains(cell))
                    {
                        selector._cellsToDeselect.Add(cell);
                        lastSelectedCell = cell;
                    }
                    else
                    {
                        selector._selectedCells.Add(cell);
                        lastSelectedCell = cell;
                    }
                }
                selector._firstSelectedCell = selector._selectedCells.FirstOrDefault();
                return lastSelectedCell;
            }
        }

        /// <summary>
        /// Implements the behavior when SHIFT is held:
        /// - If nothing was selected before, the clicked cell becomes the first selection.
        /// - Otherwise, the range between the first selected cell and the current clicked cell is determined.
        /// </summary>
        private class ShiftSelectionStrategy : ISelectionStrategy
        {
            public CellControl Preselect(CellSelector selector, MouseDownEvent evt, List<CellControl> cellsAtPosition)
            {
                CellControl lastSelectedCell = null;
                if (selector._selectedCells.Count == 0)
                {
                    selector._firstSelectedCell = cellsAtPosition.FirstOrDefault();
                    foreach (var cell in cellsAtPosition)
                    {
                        selector._selectedCells.Add(cell);
                    }
                    lastSelectedCell = selector._firstSelectedCell;
                }
                else
                {
                    var firstCell = selector._firstSelectedCell;
                    lastSelectedCell = cellsAtPosition.LastOrDefault();
                    if (firstCell != null && lastSelectedCell != null)
                    {
                        var firstRow = firstCell.Cell.Row;
                        var lastRow = lastSelectedCell.Cell.Row;
                        var firstColumn = firstCell.Cell.Column;
                        var lastColumn = lastSelectedCell.Cell.Column;
                        var cells = CellLocator.GetCellRange(selector._tableControl,
                                                             firstRow.Id, firstColumn.Id,
                                                             lastRow.Id, lastColumn.Id);
                        // Mark all cells not in the new range for deselection.
                        selector._cellsToDeselect = new HashSet<CellControl>(selector._selectedCells);
                        foreach (var cell in cells)
                        {
                            selector._selectedCells.Add(cell);
                            selector._cellsToDeselect.Remove(cell);
                        }
                    }
                }
                return lastSelectedCell;
            }
        }

        /// <summary>
        /// Implements the default (normal) selection behavior (no modifier keys):
        /// - Clears the previous selection and selects only the clicked cell(s).
        /// </summary>
        private class NormalSelectionStrategy : ISelectionStrategy
        {
            public CellControl Preselect(CellSelector selector, MouseDownEvent evt, List<CellControl> cellsAtPosition)
            {
                CellControl lastSelectedCell = null;
                // Mark all currently selected cells for deselection.
                selector._cellsToDeselect = new HashSet<CellControl>(selector._selectedCells);
                selector._firstSelectedCell = cellsAtPosition.FirstOrDefault();
                if (cellsAtPosition.Count == 1)
                {
                    lastSelectedCell = selector._firstSelectedCell;
                }
                foreach (var cell in cellsAtPosition)
                {
                    selector._selectedCells.Add(cell);
                    selector._cellsToDeselect.Remove(cell);
                    lastSelectedCell = cell;
                }
                return lastSelectedCell;
            }
        }
    }
}
