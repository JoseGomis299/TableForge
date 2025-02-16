using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellSelector : ICellSelector
    {
        private readonly TableControl _tableControl;
        private readonly HashSet<CellControl> _selectedCells = new HashSet<CellControl>();
        private HashSet<CellControl> _cellsToDeselect = new HashSet<CellControl>();
        private readonly HashSet<HeaderControl> _selectedHeaders = new HashSet<HeaderControl>();
        private Vector3 _lastMousePosition;
        
        public HashSet<CellControl> SelectedCells => _selectedCells;
        public HashSet<CellControl> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }
        public CellControl FirstSelectedCell { get; set; }

        public TableControl TableControl => _tableControl;

        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            _tableControl.RegisterCallback<MouseDownEvent>(PreselectCells, TrickleDown.TrickleDown);
            _tableControl.RegisterCallback<MouseDownEvent>(ConfirmSelection, TrickleDown.NoTrickleDown);
            _tableControl.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private bool IsValidClick(MouseDownEvent evt)
        {
            return evt.button == 0 &&
                   !_tableControl.VerticalResizer.IsResizing &&
                   !_tableControl.HorizontalResizer.IsResizing &&
                   !_tableControl.VerticalResizer.IsResizingArea(evt.mousePosition, out _) &&
                   !_tableControl.HorizontalResizer.IsResizingArea(evt.mousePosition, out _);
        }

        private void ConfirmSelection(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            ConfirmSelection();
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

                cell.IsSelected = true;
                _selectedHeaders.Add(_tableControl.ColumnHeaders[cell.Cell.Column.Id]);
                _selectedHeaders.Add(_tableControl.RowHeaders[cell.Cell.Row.Id]);
            }

            foreach (var header in _selectedHeaders)
            {
                header.IsSelected = true;
            }

            // Deselect cells marked for removal.
            foreach (var cell in _cellsToDeselect)
            {
                DeselectCell(cell);
            }
            _cellsToDeselect.Clear();
        }

        private void PreselectCells(MouseDownEvent evt)
        {
            if (!IsValidClick(evt))
                return;

            List<CellControl> cellsAtPosition = GetCellsAtPosition(evt.mousePosition);
            _lastMousePosition = evt.mousePosition;

            // Use the proper selection strategy based on modifier keys.
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy(evt);
            CellControl lastSelectedCell = strategy.Preselect(this, evt, cellsAtPosition);

            // If the last selected cell is a reference cell, immediately confirm the selection.
            if (lastSelectedCell is ReferenceCellControl)
            {
                ConfirmSelection();
            }
        }

        private List<CellControl> GetCellsAtPosition(Vector3 position)
        {
            var headers = CellLocator.GetHeadersAtPosition(_tableControl, position);
            var selectedCells = new List<CellControl>();

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
            CellControl selectedCell = null;
            if (headers.row != null && headers.column != null)
            {
                selectedCell = CellLocator.GetCell(_tableControl, headers.row.Id, headers.column.Id);
            }

            if (selectedCell == null || FirstSelectedCell == null)
                return;

            // Determine the range from the first selected cell to the cell under the cursor.
            CellControl firstCell = FirstSelectedCell;
            CellControl lastCell = selectedCell;
            var firstRow = firstCell.Cell.Row;
            var lastRow = lastCell.Cell.Row;
            var firstColumn = firstCell.Cell.Column;
            var lastColumn = lastCell.Cell.Column;
            var cells = CellLocator.GetCellRange(_tableControl, firstRow.Id, firstColumn.Id, lastRow.Id, lastColumn.Id);

            // Mark all cells currently selected for potential deselection.
            _cellsToDeselect = new HashSet<CellControl>(_selectedCells);
            foreach (var cell in cells)
            {
                _selectedCells.Add(cell);
                _cellsToDeselect.Remove(cell);
            }

            ConfirmSelection();
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
    }
}
