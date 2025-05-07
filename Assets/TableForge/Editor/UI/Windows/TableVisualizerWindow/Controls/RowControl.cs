using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;


namespace TableForge.UI
{
    internal class RowControl : VisualElement
    {
        private float _offset;
        private Task _initializationTask;
        private CancellationTokenSource _initializationCts;        
        
        public CellAnchor Anchor { get; }
        private TableControl TableControl { get; }
        
        public RowControl(CellAnchor anchor, TableControl tableControl)
        {
            TableControl = tableControl;
            Anchor = anchor;
            
            AddToClassList(USSClasses.TableRow);
        }
        
        public void ClearRow()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    CellControlFactory.Release(cell);
            }
            
            Clear();
        }

        public void RefreshColumnWidths()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cellControl)
                {
                    var column = TableControl.GetColumnHeaderControl(TableControl.GetCellColumn(cellControl.Cell));
                    cellControl.style.width = column.style.width;
                }
            }
        }
        
        public void Refresh()
        {
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                    cell.Refresh();
            }
        }

        public void ReBuild()
        {
            if(childCount > 0)
                ClearRow();
            
            if (Anchor is Row row) InitializeRow(row);
            else InitializeRow(Anchor);
            
            RefreshColumnWidths();
        }
        
        private void InitializeRow(Row row)
        {
            foreach (var columnHeader in TableControl.OrderedColumnHeaders)
            {
                if (!row.Cells.TryGetValue(columnHeader.CellAnchor.Position, out var cell) || !TableControl.ColumnHeaders[columnHeader.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                AddCell(cellField);
            }
        }
        
        private void InitializeRow(CellAnchor column)
        {
            var orderedRows = TableControl.TableData.OrderedRows;

            foreach (var row in orderedRows)
            {
                if (!row.Cells.TryGetValue(column.Position, out var cell)  || !TableControl.ColumnHeaders[row.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                AddCell(cellField);
            }
        }

        private CellControl CreateCellField(Cell cell)
        {
            var cellControl = CellControlFactory.GetPooled(cell, TableControl);
            return cellControl;
        }


        public void SetColumnVisibility(string columnId, bool isVisible, int direction)
        {
            int columnPosition = TableControl.GetColumnPosition(columnId);
            var lockedHeaders = TableControl.ColumnVisibilityManager.OrderedLockedHeaders;
            var cell = TableControl.GetCell(Anchor.Id, columnId);

            if (isVisible)
            {
                // Determine initial insertion index based on direction
                int insertIndex = (direction > 0) ? childCount : 0;

                if (lockedHeaders.Count > 0)
                {
                    // Build a quick lookup of current child positions
                    var positionIndexMap = Children()
                        .Select((ctrl, idx) => new { Pos = TableControl.GetCellColumn(((CellControl) ctrl).Cell).Position, Idx = idx })
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
                    .FirstOrDefault(ctrl => TableControl.GetCellColumn(((CellControl) ctrl).Cell).Position == columnPosition);

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

        
        private void AddCell(CellControl cell, int index = -1)
        {
            if (cell == null) return;
            
            if (index >= childCount || index == -1)
                Add(cell);
            else
                Insert(index, cell);
            
            if(cell.focusable)
                cell.SetFocused(cell.TableControl.CellSelector.IsCellFocused(cell.Cell));
        }

        private bool RefreshColumnWidthsWhileCheckingOrder()
        {
            int lastPosition = -1;
            foreach (var child in Children())
            {
                if (child is CellControl cell)
                {
                    var column = TableControl.GetColumnHeaderControl(TableControl.GetCellColumn(cell.Cell));
                    cell.style.width = column.style.width;
                    
                    int currentPosition = TableControl.GetCellColumn(cell.Cell).Position;
                    if(lastPosition >= currentPosition)
                    {
                        Debug.Log($"cell position: {currentPosition}, last position: {lastPosition}");
                        return false;
                    }
                    
                    lastPosition = currentPosition;
                }
            }
            
            return true;
        }
    }
}