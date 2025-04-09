using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.UI
{
    internal class CellSelector : ICellSelector
    {
        public event Action OnSelectionChanged;

        #region Fields

        private readonly TableControl _tableControl;
        private readonly HashSet<Cell> _selectedCells = new();
        private HashSet<Cell> _cellsToDeselect = new();
        private readonly HashSet<CellAnchor> _selectedAnchors = new();
        private Cell _firstSelectedCell;
        private List<Cell> _orderedSelectedCells = new();
        private CellSelectorInputManager _inputManager;

        #endregion

        #region Properties

        public bool SelectionEnabled { get; set; }
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;
        internal List<Cell> OrderedSelectedCells => _orderedSelectedCells;
        internal TableControl TableControl => _tableControl;

        internal HashSet<Cell> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }

        internal Cell FirstSelectedCell
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

                UpdatePreviousFirstSelected(_firstSelectedCell);
                _firstSelectedCell = value;
                if (_firstSelectedCell != null)
                {
                    UpdateNewFirstSelected(_firstSelectedCell);
                    _selectedCells.Add(_firstSelectedCell);
                    _cellsToDeselect.Remove(_firstSelectedCell);
                }
            }
        }
        
        #endregion

        #region Constructor

        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            SelectionEnabled = true;
            _inputManager = new CellSelectorInputManager(this);
        }

        #endregion

        #region FirstSelected Helpers

        private void UpdatePreviousFirstSelected(Cell previous)
        {
            if (previous == null) return;

            CellControl previousControl = CellControlFactory.GetCellControlFromId(previous.Id);
            if (previousControl != null)
            {
                previousControl.focusable = false;
                previousControl.RemoveFromClassList(USSClasses.FirstSelected);
                foreach (var ancestor in previousControl.GetAncestors(true))
                {
                    UnlockAncestorHeaders(ancestor);
                }
            }
        }

        private void UpdateNewFirstSelected(Cell newCell)
        {
            CellControl newControl = CellControlFactory.GetCellControlFromId(newCell.Id);
            if (newControl != null)
            {
                newControl.focusable = true;
                newControl.Focus();
                newControl.AddToClassList(USSClasses.FirstSelected);
                foreach (var ancestor in newControl.GetAncestors(true))
                {
                    LockAncestorHeaders(ancestor);
                }
            }
        }

        private void UnlockAncestorHeaders(CellControl ancestor)
        {
            var tableControl = ancestor.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(ancestor.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(ancestor.Cell);
            tableControl.RowVisibilityManager.UnlockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], this);
            tableControl.ColumnVisibilityManager.UnlockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], this);
        }

        private void LockAncestorHeaders(CellControl ancestor)
        {
            var tableControl = ancestor.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(ancestor.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(ancestor.Cell);
            tableControl.RowVisibilityManager.LockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], this);
            tableControl.ColumnVisibilityManager.LockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], this);
        }

        #endregion
        
        #region Selection helpers
        
        private void CollectCellsAtPosition(Vector3 position, TableControl tableControl, List<Cell> outputCells)
        {
            if (!tableControl.ScrollView.contentContainer.worldBound.Contains(position))
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
            int prevCount = outputCells.Count;

            if (cell is SubTableCell subTableCell && _selectedCells.Contains(subTableCell))
            {
                TableControl subTable = (CellControlFactory.GetCellControlFromId(subTableCell.Id) as SubTableCellControl)?.SubTableControl;
                if (subTable != null)
                {
                    CollectCellsAtPosition(position, subTable, outputCells);
                }
            }
            if ((cell != null && cell is not SubTableCell) || outputCells.Count == prevCount)
            {
                outputCells.Add(cell);
            }
        }
        
        #endregion
        
        #region Internal Methods

        internal List<Cell> GetCellsAtPosition(Vector3 position)
        {
            var output = new List<Cell>();
            CollectCellsAtPosition(position, _tableControl, output);
            return output;
        }
        internal void ConfirmSelection()
        {
            _selectedAnchors.Clear();

            // Mark selected cells and record their anchors.
            foreach (var cell in _selectedCells)
            {
                if (_cellsToDeselect.Contains(cell))
                    continue;

                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if (cellControl != null)
                    cellControl.IsSelected = true;

                _selectedAnchors.Add(cell.Row);
                _selectedAnchors.Add(cell.Column);
            }

            // Deselect cells that should be removed.
            foreach (var cell in _cellsToDeselect)
            {
                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if (cellControl != null)
                    cellControl.IsSelected = false;
                _selectedCells.Remove(cell);
            }
            _cellsToDeselect.Clear();
            
            // Order the selection differently if table is transposed.
            _orderedSelectedCells = TableControl.Transposed 
                ? _selectedCells.OrderBy(c => c.GetHighestAncestor().Column.Position)
                                .ThenBy(c => c.GetHighestAncestor().Row.Position)
                                .ThenBy(c => c.GetDepth())
                                .ThenBy(c => c.Row.Position)
                                .ThenBy(c => c.Column.Position)
                                .ToList()
                : _selectedCells.OrderBy(c => c.GetHighestAncestor().Row.Position)
                                .ThenBy(c => c.GetHighestAncestor().Column.Position)
                                .ThenBy(c => c.GetDepth())
                                .ThenBy(c => c.Row.Position)
                                .ThenBy(c => c.Column.Position)
                                .ToList();

            OnSelectionChanged?.Invoke();
        }

        internal void PreselectCells(Vector2 mousePosition, bool ctrlKey, bool shiftKey)
        {
            List<Cell> cellsAtPos = GetCellsAtPosition(mousePosition);

            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy(ctrlKey, shiftKey);
            Cell lastSelected = strategy.Preselect(this, cellsAtPos);

            // Immediately confirm selection if a reference cell was involved.
            if (lastSelected is ReferenceCell)
                ConfirmSelection();
        }
        
        internal void SelectFirstCellFromTable()
        {
            if (_tableControl.TableData.GetFirstCell() is not { } cell)
                return;
            
            FirstSelectedCell = cell;
            ConfirmSelection();
        }

        #endregion
        
        #region Public Methods

        public void ClearSelection()
        {
            _selectedCells.Clear();
            _cellsToDeselect.Clear();
            _orderedSelectedCells.Clear();
            FirstSelectedCell = null;
            ConfirmSelection();
        }

        #endregion
    }
}
