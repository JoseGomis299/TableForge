using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;


namespace TableForge.Editor.UI
{
    internal class RowControl : VisualElement
    {
        private float _offset;
        private Task _initializationTask;
        private CancellationTokenSource _initializationCts;        
        private TableControl _tableControl;

        public CellAnchor Anchor { get; private set; }
        
        public RowControl()
        {
            AddToClassList(TableVisualizerUss.TableRow);
        }
        
        public void Initialize(CellAnchor anchor, TableControl tableControl)
        {
            _tableControl = tableControl;
            Anchor = anchor;
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
                    var column = _tableControl.GetColumnHeaderControl(_tableControl.GetCellColumn(cellControl.Cell));
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
            VisualElement parentElement = parent;
            RemoveFromHierarchy();
            
            if (childCount > 0)
                ClearRow();

            if (Anchor is Row row) InitializeRow(row);
            else InitializeRow(Anchor);

            RefreshColumnWidths();
            parentElement.Insert(Anchor.Position - 1, this);
        }
        
        private void InitializeRow(Row row)
        {
            foreach (var columnHeader in _tableControl.OrderedColumnHeaders)
            {
                if (!row.Cells.TryGetValue(columnHeader.CellAnchor.Position, out var cell) || !_tableControl.ColumnHeaders[columnHeader.Id].IsVisible) continue;

                var cellField = CreateCellField(cell);
                AddCell(cellField);
            }
        }
        
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

        private CellControl CreateCellField(Cell cell)
        {
            var cellControl = CellControlFactory.GetPooled(cell, _tableControl);
            return cellControl;
        }


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

        
        private void AddCell(CellControl cell, int index = -1)
        {
            if (cell == null) return;
            
            if (index >= childCount || index == -1)
                Add(cell);
            else
                Insert(index, cell);
            
            cell.SetFocused(cell.TableControl.CellSelector.IsCellFocused(cell.Cell));
        }

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
    }
}