using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Manages cell selection in the table visualizer.
    /// Handles single and multi-selection, focus management, and selection state tracking.
    /// </summary>
    internal class CellSelector : ICellSelector
    {
        #region Events

        public event Action OnSelectionChanged;
        public event Action OnFocusedCellChanged;

        #endregion

        #region Fields

        private readonly TableControl _tableControl;
        private readonly HashSet<Cell> _selectedCells = new();
        private readonly HashSet<CellAnchor> _selectedAnchors = new();
        private readonly HashSet<CellAnchor> _subSelectedAnchors = new();
        private Cell _focusedCell;
        private readonly HashSet<int> _selectedCellIds = new();
        private readonly HashSet<int> _subSelectedAnchorIds = new();
        private CellSelectorInputManager _inputManager;
        private ICellNavigator _cellNavigator;

        #endregion

        #region Properties

        internal TableControl TableControl => _tableControl;
        internal HashSet<Cell> CellsToDeselect { get; set; } = new();
        internal HashSet<CellAnchor> AnchorsToDeselect { get; set; } = new();

        public ICellNavigator CellNavigator => _cellNavigator;
        public bool SelectionEnabled { get; set; }
        public HashSet<CellAnchor> SelectedAnchors => _selectedAnchors;
        public HashSet<Cell> SelectedCells => _selectedCells;


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
                    OnFocusedCellChanged?.Invoke();
                    return;
                }
                
                Cell previousFocusedCell = _focusedCell;
                _focusedCell = value;
                if (_focusedCell != null)
                {
                    _focusedCell.BringToView(TableControl);
                    //We need to wait for the next frame to set focus, since the cell control might need to be created, otherwise it won't work properly.
                    TableControl.schedule.Execute(() =>
                    {
                        previousFocusedCell?.SetFocused(false);
                        _focusedCell.SetFocused(true);
                    }).ExecuteLater(0);
                
                    
                    _selectedCells.Add(_focusedCell);
                    CellsToDeselect.Remove(_focusedCell);
                }
                else previousFocusedCell?.SetFocused(false);
                
                OnFocusedCellChanged?.Invoke();
              //  UndoRedoManager.AddSeparator();
            }
        }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CellSelector class.
        /// </summary>
        /// <param name="tableControl">The table control that owns this selector.</param>
        public CellSelector(TableControl tableControl)
        {
            _tableControl = tableControl;
            SelectionEnabled = true;
            _inputManager = new CellSelectorInputManager(this);
        }

        #endregion
        
        #region Internal Methods - Selection Management

        /// <summary>
        /// Gets cell preselect arguments for a given position.
        /// </summary>
        /// <param name="position">The position to check for cells.</param>
        /// <returns>Preselect arguments containing cells and anchors at the position.</returns>
        internal PreselectArguments GetCellPreselectArgsForPosition(Vector3 position)
        {
            var output = new PreselectArguments(this);
            CollectCellsAtPosition(position, _tableControl, output);
            return output;
        }

        /// <summary>
        /// Confirms the current selection and updates the UI.
        /// </summary>
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

                _subSelectedAnchors.Add(cell.row);
                _subSelectedAnchors.Add(cell.column);
                
                _selectedCellIds.Add(cell.Id);
                _subSelectedAnchorIds.Add(cell.row.Id);
                _subSelectedAnchorIds.Add(cell.column.Id);
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

        /// <summary>
        /// Preselects cells based on mouse position and modifier keys.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        /// <param name="ctrlKey">Whether the Ctrl key is pressed.</param>
        /// <param name="shiftKey">Whether the Shift key is pressed.</param>
        /// <param name="isLeftClick">Whether this is a left click.</param>
        internal void PreselectCells(Vector2 mousePosition, bool ctrlKey, bool shiftKey, bool isLeftClick)
        {
            PreselectArguments preselectArgs = GetCellPreselectArgsForPosition(mousePosition);
            if(!isLeftClick && (preselectArgs.selectedAnchors.Count == 0 || shiftKey || ctrlKey)) 
                return;
            
            if(_selectedCells.Contains(preselectArgs.cellsAtPosition.LastOrDefault()) && preselectArgs.clickedOnToolbar)
                return;
            
            preselectArgs.rightClicked = !isLeftClick;

            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy(ctrlKey, shiftKey);
            strategy.Preselect(preselectArgs);

            TableControl.schedule.Execute(ConfirmSelection).ExecuteLater(0);
        }
        
        /// <summary>
        /// Selects the first cell from the table.
        /// </summary>
        internal void SelectFirstCellFromTable()
        {
            if (_tableControl.TableData.GetFirstCell() is not { } cell)
                return;
            
            FocusedCell = cell;
            ConfirmSelection();
        }
        
        /// <summary>
        /// Selects a range of cells between two cells.
        /// </summary>
        /// <param name="firstCell">The first cell in the range.</param>
        /// <param name="lastCell">The last cell in the range.</param>
        internal void SelectRange(Cell firstCell, Cell lastCell)
        {
            if (firstCell == null || lastCell == null)
                return;

            FocusedCell = firstCell;
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<MultipleSelectionStrategy>();
            strategy.Preselect(new PreselectArguments
            {
                selector = this,
                cellsAtPosition = new List<Cell> { lastCell }
            });
            
            ConfirmSelection();
        }
        
        /// <summary>
        /// Selects a specific list of cells.
        /// </summary>
        /// <param name="cells">The cells to select.</param>
        internal void SelectRange(IList<Cell> cells)
        {
            if (cells == null || cells.Count == 0)
                return;

            FocusedCell = cells.FirstOrDefault();
            ISelectionStrategy strategy = SelectionStrategyFactory.GetSelectionStrategy<MultipleSelectionStrategy>();
            strategy.Preselect(new PreselectArguments
            {
                selector = this,
                cellsAtPosition = new List<Cell>(cells)
            });
            
            ConfirmSelection();
        }

        #endregion

        #region Public Methods - Selection State

        /// <summary>
        /// Sets the selection to a new list of cells.
        /// </summary>
        /// <param name="newSelection">The new selection.</param>
        /// <param name="setFocused">Whether to set the focused cell to the first cell in the selection.</param>
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

        /// <summary>
        /// Sets the focused cell.
        /// </summary>
        /// <param name="cell">The cell to focus.</param>
        public void SetFocusedCell(Cell cell)
        {
            FocusedCell = cell;
        }
        
        /// <summary>
        /// Gets the currently focused cell.
        /// </summary>
        /// <returns>The focused cell, or null if no cell is focused.</returns>
        public Cell GetFocusedCell()
        {
            return _focusedCell;
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

        #endregion

        #region Public Methods - Selection Operations

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            _selectedCells.Clear();
            CellsToDeselect.Clear();
            FocusedCell = null;
            ConfirmSelection();
        }

        /// <summary>
        /// Clears selection for cells from a specific table.
        /// </summary>
        /// <param name="fromTable">The table to clear selection from.</param>
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

        /// <summary>
        /// Gets selected rows from a specific table.
        /// </summary>
        /// <param name="fromTable">The table to get rows from.</param>
        /// <returns>A list of selected rows.</returns>
        public List<Row> GetSelectedRows(Table fromTable) => _selectedAnchors.OfType<Row>().Where(r => r.Table == fromTable).ToList();
        
        /// <summary>
        /// Gets selected columns from a specific table.
        /// </summary>
        /// <param name="fromTable">The table to get columns from.</param>
        /// <returns>A list of selected columns.</returns>
        public List<Column> GetSelectedColumns(Table fromTable) => _selectedAnchors.OfType<Column>().Where(c => c.Table == fromTable).ToList();
        
        /// <summary>
        /// Gets selected cells from a specific table.
        /// </summary>
        /// <param name="fromTable">The table to get cells from, null to get all selected cells.</param>
        /// <returns>A list of selected cells.</returns>
        public List<Cell> GetSelectedCells(Table fromTable) => _selectedCells.Where(c => fromTable == null || c.Table == fromTable).ToList();
        
        /// <summary>
        /// Removes selection from a specific row.
        /// </summary>
        /// <param name="row">The row to remove selection from.</param>
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

        #region Private Methods - Cell Collection

        /// <summary>
        /// Collects cells at a specific position for selection.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="tableControl">The table control to search in.</param>
        /// <param name="outputArgs">The output arguments to populate.</param>
        private void CollectCellsAtPosition(Vector3 position, TableControl tableControl, PreselectArguments outputArgs)
        {
            if (!tableControl.ScrollView.contentViewport.worldBound.Contains(position))
            {
                outputArgs.clickedOnToolbar = tableControl.SubTableToolbar != null &&
                                              tableControl.SubTableToolbar.worldBound.Contains(position);
                return;
            }

            //If clicking on the corner of the table, select all cells.
            if (tableControl.CornerContainer.worldBound.Contains(position))
            {
                foreach (var row in tableControl.TableData.OrderedRows)
                {
                    if(!TableControl.Filterer.IsVisible(row.GetRootAnchor().Id)) continue;

                    foreach (var rowCell in row.OrderedCells)
                    {
                        if(!TableControl.Metadata.IsFieldVisible(rowCell.column.GetRootAnchor().Id)) continue;
                        outputArgs.cellsAtPosition.Add(rowCell);
                    }
                    
                    outputArgs.selectedAnchors.Add(row);
                    _selectedAnchors.Add(row);
                }
                
                foreach (var column in tableControl.TableData.OrderedColumns)
                {
                    if(!TableControl.Metadata.IsFieldVisible(column.GetRootAnchor().Id)) continue;
                    
                    outputArgs.selectedAnchors.Add(column);
                    _selectedAnchors.Add(column);
                }
                return;
            }

            var headers = CellLocator.GetHeadersAtPosition(tableControl, position);
            if (headers.row == null && headers.column == null)
                return;

            //If clicking on a column header, select all cells in that column.
            if (headers.row == null)
            {
                if(!TableControl.Metadata.IsFieldVisible(headers.column.Id)) return;
                
                outputArgs.cellsAtPosition.AddRange(CellLocator.GetCellsAtColumn(tableControl, headers.column.Id));
                outputArgs.selectedAnchors.Add(headers.column.CellAnchor);
                _selectedAnchors.Add(headers.column.CellAnchor);
                return;
            }
            
            //If clicking on a row header, select all cells in that row.
            if (headers.column == null)
            {
                if(!TableControl.Filterer.IsVisible(headers.row.CellAnchor.GetRootAnchor().Id)) return;
                
                outputArgs.cellsAtPosition.AddRange(CellLocator.GetCellsAtRow(tableControl, headers.row.Id));
                outputArgs.selectedAnchors.Add(headers.row.CellAnchor);
                _selectedAnchors.Add(headers.row.CellAnchor);
                return;
            }

            //If clicking on a cell, select that cell.
            var cell = CellLocator.GetCell(tableControl, headers.row.Id, headers.column.Id);
            int prevCount =  outputArgs.cellsAtPosition.Count;

            if (cell is SubTableCell subTableCell && _selectedCells.Contains(subTableCell))
            {
                TableControl subTable = (CellControlFactory.GetCellControlFromId(subTableCell.Id) as SubTableCellControl)?.SubTableControl;
                if (subTable != null)
                {
                    CollectCellsAtPosition(position, subTable, outputArgs);
                }
            }
            if (cell is not SubTableCell ||  outputArgs.cellsAtPosition.Count == prevCount)
            {
                if(cell == null || !TableControl.Metadata.IsFieldVisible(cell.column.Id)) return;
                outputArgs.cellsAtPosition.Add(cell);
            }
        }

        #endregion
    }
}
