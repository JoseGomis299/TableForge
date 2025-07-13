using System.Linq;
using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;


namespace TableForge.Editor.UI
{
    /// <summary>
    /// Controls the visual representation and behavior of a table row in the TableForge visualizer.
    /// Manages cell creation, visibility, and layout within a row.
    /// </summary>
    internal class RowControl : VisualElement
    {
        #region Private Fields

        private float _offset;
        private TableControl _tableControl;

        #endregion

        #region Properties

        public CellAnchor Anchor { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RowControl class.
        /// </summary>
        public RowControl()
        {
            AddToClassList(TableVisualizerUss.TableRow);
        }

        #endregion

        #region Public Methods - Initialization

        /// <summary>
        /// Initializes the row control with the specified anchor and table control.
        /// </summary>
        /// <param name="anchor">The cell anchor representing this row.</param>
        /// <param name="tableControl">The parent table control.</param>
        public void Initialize(CellAnchor anchor, TableControl tableControl)
        {
            _tableControl = tableControl;
            Anchor = anchor;
        }

        #endregion

        #region Public Methods - Row Management

        /// <summary>
        /// Clears all cells from the row and releases them back to the pool.
        /// </summary>
        public void ClearRow()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    CellControlFactory.Release(cell);
            }
            
            Clear();
        }

        /// <summary>
        /// Refreshes the width of all cells in the row to match their corresponding column headers.
        /// </summary>
        public void RefreshColumnWidths()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cellControl)
                {
                    var column = _tableControl.GetColumnHeaderControl(_tableControl.GetCellColumn(cellControl.Cell));
                    cellControl.style.width = column.style.width;
                }
            }
        }
        
        /// <summary>
        /// Refreshes all cells in the row to update their visual state.
        /// </summary>
        public void Refresh()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    cell.Refresh();
            }
        }

        /// <summary>
        /// Rebuilds the entire row by clearing existing cells and recreating them.
        /// Maintains the correct position in the parent hierarchy.
        /// </summary>
        public void ReBuild()
        {
            VisualElement parentElement = parent;
            RemoveFromHierarchy();
            
            if (childCount > 0)
                ClearRow();

            if (Anchor is Row row) InitializeRow(row);
            else InitializeRow(Anchor);

            RefreshColumnWidths();
            parentElement.Insert(Anchor.Position - 1, this);
        }

        /// <summary>
        /// Shows or hides a column in this row based on visibility changes.
        /// Handles complex insertion logic for locked headers and maintains proper cell ordering.
        /// </summary>
        /// <param name="columnId">The ID of the column to show or hide.</param>
        /// <param name="isVisible">Whether the column should be visible.</param>
        /// <param name="direction">The direction of the visibility change (positive for showing, negative for hiding).</param>
        public void ShowColumn(int columnId, bool isVisible, int direction)
        {
            int columnPosition = _tableControl.GetColumnPosition(columnId);
            var lockedHeaders = _tableControl.ColumnVisibilityManager.OrderedLockedHeaders;
            var cell = _tableControl.GetCell(Anchor.Id, columnId);

            if (isVisible)
            {
                // Determine initial insertion index based on direction
                int insertIndex = (direction > 0) ? childCount : 0;

                if (lockedHeaders.Count > 0)
                {
                    // Build a quick lookup of current child positions
                    var positionIndexMap = Children()
                        .Select((ctrl, idx) => new { Pos = _tableControl.GetCellColumn(((CellControl) ctrl).Cell).Position, Idx = idx })
                        .ToDictionary(x => x.Pos, x => x.Idx);

                    // Adjust insertion index relative to locked headers
                    foreach (var locked in lockedHeaders)
                    {
                        if (!positionIndexMap.TryGetValue(locked.CellAnchor.Position, out var lockedIdx))
                            continue;

                        int lockedPos = locked.CellAnchor.Position;

                        if (lockedPos < columnPosition && insertIndex <= lockedIdx)
                        {
                            insertIndex = lockedIdx + 1;
                        }
                        else if (lockedPos > columnPosition && insertIndex > lockedIdx)
                        {
                            insertIndex = lockedIdx;
                        }
                    }
                }

                // Create and insert the new cell
                var newCell = CreateCellField(cell);
                AddCell(newCell, insertIndex);
            }
            else
            {
                // Remove the cell control if it exists
                var toRemove = Children()
                    .FirstOrDefault(ctrl => _tableControl.GetCellColumn(((CellControl) ctrl).Cell).Position == columnPosition);

                if (toRemove != null)
                {
                    Remove(toRemove);
                    CellControlFactory.Release((CellControl) toRemove);
                }
            }

            // Ensure widths are correct and order is valid
            if (!RefreshColumnWidthsWhileCheckingOrder())
                ReBuild();
        }

        #endregion

        #region Private Methods - Row Initialization

        /// <summary>
        /// Initializes the row with cells from a Row object.
        /// Creates cell controls for each visible column that has data.
        /// </summary>
        /// <param name="row">The row object containing cell data.</param>
        private void InitializeRow(Row row)
        {
            foreach (var columnHeader in _tableControl.OrderedColumnHeaders)
            {
                if (!row.Cells.TryGetValue(columnHeader.CellAnchor.Position, out var cell) || !_tableControl.ColumnHeaders[columnHeader.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                AddCell(cellField);
            }
        }
        
        /// <summary>
        /// Initializes the row with cells from a Column anchor (for transposed tables).
        /// Creates cell controls for each row that has data in the specified column.
        /// </summary>
        /// <param name="column">The column anchor representing the transposed row.</param>
        private void InitializeRow(CellAnchor column)
        {
            var orderedRows = _tableControl.TableData.OrderedRows;

            foreach (var row in orderedRows)
            {
                if (!row.Cells.TryGetValue(column.Position, out var cell)  || !_tableControl.ColumnHeaders[row.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                AddCell(cellField);
            }
        }

        #endregion

        #region Private Methods - Cell Management

        /// <summary>
        /// Creates a cell control for the specified cell using the factory pattern.
        /// </summary>
        /// <param name="cell">The cell data to create a control for.</param>
        /// <returns>A pooled cell control instance.</returns>
        private CellControl CreateCellField(Cell cell)
        {
            var cellControl = CellControlFactory.GetPooled(cell, _tableControl);
            return cellControl;
        }

        /// <summary>
        /// Adds a cell control to the row at the specified index or at the end if no index is provided.
        /// Sets the focus state of the cell based on the table's cell selector.
        /// </summary>
        /// <param name="cell">The cell control to add.</param>
        /// <param name="index">The index to insert at, or -1 to add at the end.</param>
        private void AddCell(CellControl cell, int index = -1)
        {
            if (cell == null) return;
            
            if (index >= childCount || index == -1)
                Add(cell);
            else
                Insert(index, cell);
            
            cell.SetFocused(cell.TableControl.CellSelector.IsCellFocused(cell.Cell));
        }

        #endregion

        #region Private Methods - Validation and Ordering

        /// <summary>
        /// Refreshes column widths while checking if the cell order is correct.
        /// Returns false if cells are out of order, indicating a rebuild is needed.
        /// </summary>
        /// <returns>True if the order is correct, false if a rebuild is needed.</returns>
        private bool RefreshColumnWidthsWhileCheckingOrder()
        {
            int lastPosition = -1;
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                {
                    var column = _tableControl.GetColumnHeaderControl(_tableControl.GetCellColumn(cell.Cell));
                    cell.style.width = column.style.width;
                    
                    int currentPosition = _tableControl.GetCellColumn(cell.Cell).Position;
                    if(lastPosition >= currentPosition)
                    {
                        return false;
                    }
                    
                    lastPosition = currentPosition;
                }
            }
            
            return true;
        }

        #endregion
    }
}