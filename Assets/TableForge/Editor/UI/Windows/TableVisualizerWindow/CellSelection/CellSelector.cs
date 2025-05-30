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
        private readonly HashSet<CellAnchor> _selectedAnchors = new();
        private readonly HashSet<CellAnchor> _subSelectedAnchors = new();
        private Cell _focusedCell;
        private readonly HashSet<int> _selectedCellIds = new();
        private readonly HashSet<int> _subSelectedAnchorIds  = new();
        private CellSelectorInputManager _inputManager;
        private ICellNavigator _cellNavigator;

        #endregion

        #region Properties

        public ICellNavigator CellNavigator => _cellNavigator;
        public bool SelectionEnabled { get; set; }
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;
        internal TableControl TableControl => _tableControl;

        internal HashSet<Cell> CellsToDeselect { get; set; } = new();
        internal HashSet<CellAnchor> AnchorsToDeselect { get; set; } = new();

        public Cell FocusedCell
        {
            get => _focusedCell;
            set
            {
                if (_focusedCell == value)
                {
                    if (value == null) return;
                    _selectedCells.Add(value);
                    CellsToDeselect.Remove(value);
                    return;
                }

                _focusedCell?.SetFocused(false);
                _focusedCell = value;
                if (_focusedCell != null)
                {
                    _focusedCell.BringToView(TableControl);
                    _focusedCell.SetFocused(true);
                    
                    _selectedCells.Add(_focusedCell);
                    CellsToDeselect.Remove(_focusedCell);
                }
                
                UndoRedoManager.AddSeparator();
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
        
        private void CollectCellsAtPosition(Vector3 position, TableControl tableControl, PreselectArguments outputArgs)
        {
            if (!tableControl.ScrollView.contentViewport.worldBound.Contains(position))
            {
                outputArgs.ClickedOnToolbar = tableControl.SubTableToolbar != null &&
                                              tableControl.SubTableToolbar.worldBound.Contains(position);
                return;
            }

            //If clicking on the corner of the table, select all cells.
            if (tableControl.CornerContainer.worldBound.Contains(position))
            {
                foreach (var row in tableControl.TableData.OrderedRows)
                    outputArgs.CellsAtPosition.AddRange(row.OrderedCells);
                return;
            }

            var headers = CellLocator.GetHeadersAtPosition(tableControl, position);
            if (headers.row == null && headers.column == null)
                return;

            //If clicking on a column header, select all cells in that column.
            if (headers.row == null)
            {
                if(!TableControl.Metadata.IsFieldVisible(headers.column.Id)) return;
                
                outputArgs.CellsAtPosition.AddRange(CellLocator.GetCellsAtColumn(tableControl, headers.column.Id));
                outputArgs.SelectedAnchors.Add(headers.column.CellAnchor);
                _selectedAnchors.Add(headers.column.CellAnchor);
                return;
            }
            
            //If clicking on a row header, select all cells in that row.
            if (headers.column == null)
            {
                outputArgs.CellsAtPosition.AddRange(CellLocator.GetCellsAtRow(tableControl, headers.row.Id));
                outputArgs.SelectedAnchors.Add(headers.row.CellAnchor);
                _selectedAnchors.Add(headers.row.CellAnchor);
                return;
            }

            //If clicking on a cell, select that cell.
            var cell = CellLocator.GetCell(tableControl, headers.row.Id, headers.column.Id);
            int prevCount =  outputArgs.CellsAtPosition.Count;

            if (cell is SubTableCell subTableCell && _selectedCells.Contains(subTableCell))
            {
                TableControl subTable = (CellControlFactory.GetCellControlFromId(subTableCell.Id) as SubTableCellControl)?.SubTableControl;
                if (subTable != null)
                {
                    CollectCellsAtPosition(position, subTable, outputArgs);
                }
            }
            if (cell is not SubTableCell ||  outputArgs.CellsAtPosition.Count == prevCount)
            {
                if(cell == null || !TableControl.Metadata.IsFieldVisible(cell.Column.Id)) return;
                outputArgs.CellsAtPosition.Add(cell);
            }
        }
        
        #endregion
        
        #region Internal Methods

        internal PreselectArguments GetCellPreselectArgsForPosition(Vector3 position)
        {
            var output = new PreselectArguments(this);
            CollectCellsAtPosition(position, _tableControl, output);
            return output;
        }
        internal void ConfirmSelection()
        {
            _subSelectedAnchors.Clear();
            _selectedCellIds.Clear();
            _subSelectedAnchorIds.Clear();
            
            // Mark selected cells and select their anchors.
            foreach (var cell in _selectedCells)
            {
                if (CellsToDeselect.Contains(cell))
                    continue;

                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if (cellControl != null)
                    cellControl.IsSelected = true;

                _subSelectedAnchors.Add(cell.Row);
                _subSelectedAnchors.Add(cell.Column);
                
                _selectedCellIds.Add(cell.Id);
                _subSelectedAnchorIds.Add(cell.Row.Id);
                _subSelectedAnchorIds.Add(cell.Column.Id);
            }

            // Deselect cells that should be removed.
            foreach (var cell in CellsToDeselect)
            {
                CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
                if (cellControl != null)
                    cellControl.IsSelected = false;
                _selectedCells.Remove(cell);
            }
            CellsToDeselect.Clear();
            
            // Deselect anchors that should be removed from the selection.
            foreach (var anchor in AnchorsToDeselect)
            {
                _selectedAnchors.Remove(anchor);
            }
            AnchorsToDeselect.Clear();
            foreach (var anchor in _selectedAnchors.ToList())
            {
                if(!_subSelectedAnchors.Contains(anchor))
                    _selectedAnchors.Remove(anchor);
            }
            
            // Order the selection differently if table is transposed.
            _cellNavigator = new ConfinedSpaceNavigator(_selectedCells.ToList(), _tableControl.Metadata, _focusedCell);
            OnSelectionChanged?.Invoke();
        }

        internal void PreselectCells(Vector2 mousePosition, bool ctrlKey, bool shiftKey, bool isLeftClick)
        {
            PreselectArguments preselectArgs = GetCellPreselectArgsForPosition(mousePosition);
            if(!isLeftClick && (preselectArgs.SelectedAnchors.Count == 0 || shiftKey || ctrlKey)) 
                return;
            
            if(_selectedCells.Contains(preselectArgs.CellsAtPosition.LastOrDefault()) && preselectArgs.ClickedOnToolbar)
                return;
            
            preselectArgs.RightClicked = !isLeftClick;

            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy(ctrlKey, shiftKey);
            strategy.Preselect(preselectArgs);

            TableControl.schedule.Execute(ConfirmSelection).ExecuteLater(0);
        }
        
        internal void SelectFirstCellFromTable()
        {
            if (_tableControl.TableData.GetFirstCell() is not { } cell)
                return;
            
            FocusedCell = cell;
            ConfirmSelection();
        }
        
        internal void SelectRange(Cell firstCell, Cell lastCell)
        {
            if (firstCell == null || lastCell == null)
                return;

            FocusedCell = firstCell;
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<MultipleSelectionStrategy>();
            strategy.Preselect(new PreselectArguments
            {
                Selector = this,
                CellsAtPosition = new List<Cell> { firstCell }
            });
            
            ConfirmSelection();
        }

        #endregion
        
        #region Public Methods

        public void SetSelection(List<Cell> newSelection, bool setFocused = true)
        {
            if (newSelection == null || newSelection.Count == 0)
            {
                ClearSelection();
                return;
            }

            _selectedCells.Clear();

            foreach (var cell in newSelection)
            {
                _selectedCells.Add(cell);
            }

            if(setFocused)
                FocusedCell = newSelection.FirstOrDefault();
            ConfirmSelection();
        }

        public void SetFocusedCell(Cell cell)
        {
            FocusedCell = cell;
        }

        public bool IsCellSelected(Cell cell)
        {
            return cell != null && _selectedCellIds.Contains(cell.Id);
        }

        public bool IsAnchorSelected(CellAnchor cellAnchor)
        {
            return cellAnchor != null && _selectedAnchors.Contains(cellAnchor);
        }
        
        public bool IsAnchorSubSelected(CellAnchor cellAnchor)
        {
            return cellAnchor != null && _subSelectedAnchorIds.Contains(cellAnchor.Id);
        }

        public bool IsCellFocused(Cell cell)
        {
            return _focusedCell != null && _focusedCell.Id == cell.Id;
        }
        
        public void ClearSelection()
        {
            _selectedCells.Clear();
            CellsToDeselect.Clear();
            FocusedCell = null;
            ConfirmSelection();
        }

        public void ClearSelection(Table fromTable)
        {
            if (fromTable == null)
                return;

            foreach (var cell in _selectedCells)
            {
                if (cell.Table == fromTable)
                    CellsToDeselect.Add(cell);
            }
            
            ConfirmSelection();
            
            if (!_selectedCells.Contains(_focusedCell))
               FocusedCell = _selectedCells.FirstOrDefault(); 
        }

        public List<Row> GetSelectedRows() => _selectedAnchors.OfType<Row>().ToList();
        public List<Column> GetSelectedColumns() => _selectedAnchors.OfType<Column>().ToList();
        public void RemoveRowSelection(Row row)
        {
            if (row == null)
                return;
            
            foreach (var cell in row.OrderedCells)
            {
                CellsToDeselect.Add(cell);
            }

            AnchorsToDeselect.Add(row);
            ConfirmSelection();
        }

        #endregion
    }
}
