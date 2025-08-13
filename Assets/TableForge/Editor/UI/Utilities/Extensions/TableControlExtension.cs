using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal static class TableControlExtension
    {
        private const int SubTableMinScrollDiff = 10;

        public static CellAnchor GetRowAtPosition(this TableControl tableControl, int position)
        {
            if (!tableControl.Transposed)
            {
                if (tableControl.TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }
            else
            {
                if (tableControl.TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }

            return null;
        }
        
        public static CellAnchor GetColumnAtPosition(this TableControl tableControl, int position)
        {
            if (!tableControl.Transposed)
            {
                if (tableControl.TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }
            else
            {
                if (tableControl.TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }

            return null;
        }

        public static CellAnchor GetCellRow(this TableControl tableControl, Cell cell)
        {
            return !tableControl.Transposed ? cell.row : cell.column;
        }
        
        public static CellAnchor GetCellColumn(this TableControl tableControl, Cell cell)
        {
            return !tableControl.Transposed ? cell.column : cell.row;
        }

        public static int GetColumnPosition(this TableControl tableControl, int columnId)
        {
            if (!tableControl.ColumnData.ContainsKey(columnId))
                return -1;

            return tableControl.ColumnData[columnId].Position;
        }
        
        public static RowHeaderControl GetRowHeaderControl(this TableControl tableControl, CellAnchor row)
        {
            return tableControl.RowHeaders[row.Id];
        }
        
        public static ColumnHeaderControl GetColumnHeaderControl(this TableControl tableControl, CellAnchor column)
        {
            return tableControl.ColumnHeaders[column.Id];
        }
        
        public static TableControl GetRootTableControl(this TableControl tableControl)
        {
            return tableControl.Parent == null ? tableControl : tableControl.Parent.TableControl.GetRootTableControl();
        }

        public static int GetId(this TableControl tableControl)
        {
           return tableControl.TableData.IsSubTable ? tableControl.TableData.ParentCell.Id : 0;
        }
        
        public static Cell GetCell(this TableControl tableControl, int rowId, int columnId)
        {
            if (!tableControl.RowData.ContainsKey(rowId) || !tableControl.ColumnData.ContainsKey(columnId))
                return null;

            if (tableControl.RowData[rowId] is Row row)
            {
                if (row.Cells.TryGetValue(tableControl.ColumnData[columnId].Position, out var cell))
                    return cell;
            }
            else if (tableControl.ColumnData[columnId] is Row column)
            {
                if (column.Cells.TryGetValue(tableControl.RowData[rowId].Position, out var cell))
                    return cell;
            }

            return null;
        }

        public static CellControl GetCellControl(this TableControl tableControl, int rowId, int columnId)
        {
            Cell cell = tableControl.GetCell(rowId, columnId);
            return CellControlFactory.GetCellControlFromId(cell.Id);
        }
        
          public static void SetScrollbarsVisibility(this TableControl tableControl, bool show)
        {
            ScrollView scrollView = tableControl.ScrollView;
            
            if (show)
            {
                AdjustVerticalScroller(tableControl);
                AdjustHorizontalScroller(tableControl);
            }
            else
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }
        
        public static void SetVerticalScrollerMaxValue(this TableControl tableControl, float value)
        {
            ScrollView scrollView = tableControl.ScrollView;

            scrollView.verticalScroller.highValue = value - scrollView.contentViewport.resolvedStyle.height;
            scrollView.verticalScroller.value = Mathf.Min(value, scrollView.verticalScroller.value);
            
            AdjustVerticalScroller(tableControl, value);
        }

        public static void AdjustVerticalScroller(this TableControl tableControl, float scrollViewHeight = -1)
        {
            ScrollView scrollView = tableControl.ScrollView;
            
            float viewportHeight = scrollView.contentViewport.resolvedStyle.height;
            if(scrollView.horizontalScroller.visible && scrollView.horizontalScrollerVisibility != ScrollerVisibility.Hidden)
                viewportHeight += scrollView.horizontalScroller.resolvedStyle.height;
            
            if(scrollViewHeight == -1) 
                scrollViewHeight = scrollView.contentContainer.resolvedStyle.height;
            
            float scrollerFactor = viewportHeight / scrollViewHeight;
            if (!VerticalScrollerShouldBeVisible(tableControl, scrollViewHeight))
            {
                scrollView.verticalScroller.Adjust(1);
                scrollView.verticalScroller.visible = false;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScroller.style.display = DisplayStyle.None;
            }
            else 
            {
                scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
                scrollView.verticalScroller.visible = true;
                scrollView.verticalScroller.Adjust(scrollerFactor);
                scrollView.verticalScroller.style.display = DisplayStyle.Flex;
            }
        }
        
        public static bool VerticalScrollerShouldBeVisible(this TableControl tableControl, float scrollViewHeight)
        {
            if(scrollViewHeight == 0) 
                return false;
            
            ScrollView scrollView = tableControl.ScrollView;
            int maxDiff = 0;
            if (tableControl.Parent != null)
                maxDiff = SubTableMinScrollDiff;
            
            int pixelDifference = (int)(scrollViewHeight - scrollView.contentViewport.resolvedStyle.height);
            return pixelDifference > maxDiff;
        }

        public static void SetHorizontalScrollerMaxValue(this TableControl tableControl, float value)
        {
            ScrollView scrollView = tableControl.ScrollView;

            scrollView.horizontalScroller.highValue = value - scrollView.contentViewport.resolvedStyle.width;
            scrollView.horizontalScroller.value = Mathf.Min(value, scrollView.horizontalScroller.value);
            
            AdjustHorizontalScroller(tableControl, value);
        }

        public static void AdjustHorizontalScroller(this TableControl tableControl, float scrollViewWidth = -1)
        {
            ScrollView scrollView = tableControl.ScrollView;
            
            if(scrollViewWidth == -1) 
                scrollViewWidth = scrollView.contentContainer.resolvedStyle.width;
            
            float scrollerFactor = scrollView.contentViewport.resolvedStyle.width / scrollViewWidth;
            if (!HorizontalScrollerShouldBeVisible(tableControl, scrollViewWidth))
            {
                scrollView.horizontalScroller.Adjust(1);
                scrollView.horizontalScroller.visible = false;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.horizontalScroller.style.display = DisplayStyle.None;
            }
            else 
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
                scrollView.horizontalScroller.visible = true;
                scrollView.horizontalScroller.Adjust(scrollerFactor);
                scrollView.horizontalScroller.style.display = DisplayStyle.Flex;
            }
        }

        public static bool HorizontalScrollerShouldBeVisible(this TableControl tableControl, float scrollViewWidth)
        {
            if(scrollViewWidth == 0) 
                return false;
            
            ScrollView scrollView = tableControl.ScrollView;
            int maxDiff = 0;
            if (tableControl.Parent != null)
                maxDiff = SubTableMinScrollDiff;

            int pixelDifference = (int)(scrollViewWidth - scrollView.contentViewport.resolvedStyle.width);
            return pixelDifference > maxDiff;
        }

        public static void ResizeAllRecursively(this TableControl tableControl, bool fitStoredSize, bool storeSize)
        {
            if (tableControl == null) return;

            if (!tableControl.Transposed)
            {
                foreach (var row in tableControl.RowHeaders.Values)
                {
                    if (tableControl.RowVisibilityManager.IsHeaderVisible(row))
                        UpdateChildren(row);
                }
            }
            else
            {
                foreach (var column in tableControl.ColumnHeaders.Values)
                {
                    if (tableControl.ColumnVisibilityManager.IsHeaderVisible(column))
                        UpdateChildren(column);
                }
            }

            tableControl.Resizer.ResizeAll(fitStoredSize, storeSize);

            void UpdateChildren(HeaderControl row)
            {
                foreach (var cell in ((Row)row.CellAnchor).OrderedCells)
                {
                    if (cell is SubTableCell)
                    {
                        if (CellControlFactory.GetCellControlFromId(cell.Id) is SubTableCellControl
                            {
                                SubTableControl: not null
                            } subTableCellControl)
                        {
                            subTableCellControl.SubTableControl.RebuildPage();
                            subTableCellControl.SubTableControl.ResizeAllRecursively(fitStoredSize, storeSize);
                        }
                    }
                }
            }
        }
    }
}