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
        private Cell _focusedCell;
        private readonly HashSet<int> _selectedCellIds = new();
        private readonly HashSet<int> _selectedAnchorIds  = new();
        private CellSelectorInputManager _inputManager;

        #endregion

        #region Properties

        public bool SelectionEnabled { get; set; }
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;
        internal CellNavigator CellNavigator { get; } = new();
        internal TableControl TableControl => _tableControl;

        internal HashSet<Cell> CellsToDeselect
        {
            get => _cellsToDeselect;
            set => _cellsToDeselect = value;
        }

        public Cell FocusedCell
        {
            get => _focusedCell;
            set
            {
                if (_focusedCell == value)
                {
                    if (value == null) return;
                    _selectedCells.Add(value);
                    _cellsToDeselect.Remove(value);
                    return;
                }

                _focusedCell?.SetFocused(false);
                _focusedCell = value;
                if (_focusedCell != null)
                {
                    _focusedCell.BringToView(TableControl);
                    _focusedCell.SetFocused(true);
                    
                    _selectedCells.Add(_focusedCell);
                    _cellsToDeselect.Remove(_focusedCell);
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
        
        #region Selection helpers
        
        private void CollectCellsAtPosition(Vector3 position, TableControl tableControl, List<Cell> outputCells)
        {
            if (!tableControl.ScrollView.contentViewport.worldBound.Contains(position))
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
            _selectedCellIds.Clear();
            _selectedAnchorIds.Clear();

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
                
                _selectedCellIds.Add(cell.Id);
                _selectedAnchorIds.Add(cell.Row.Id);
                _selectedAnchorIds.Add(cell.Column.Id);
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
            CellNavigator.SetNavigationSpace(_selectedCells.ToList(), _tableControl.Metadata, _focusedCell);
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
            
            FocusedCell = cell;
            ConfirmSelection();
        }

        #endregion
        
        #region Public Methods

        public bool IsCellSelected(Cell cell)
        {
            return cell != null && _selectedCellIds.Contains(cell.Id);
        }

        public bool IsAnchorSelected(CellAnchor cellAnchor)
        {
            return cellAnchor != null && _selectedAnchorIds.Contains(cellAnchor.Id);
        }

        public bool IsCellFocused(Cell cell)
        {
            return _focusedCell != null && _focusedCell.Id == cell.Id;
        }
        
        public void ClearSelection()
        {
            _selectedCells.Clear();
            _cellsToDeselect.Clear();
            CellNavigator.Clear();
            FocusedCell = null;
            ConfirmSelection();
        }

        #endregion
    }
}
