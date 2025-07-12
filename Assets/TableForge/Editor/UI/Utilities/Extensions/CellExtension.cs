using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class CellExtension
    {
        /// <summary>
        /// Gets the direction from one cell to another in the table hierarchy.
        /// </summary>
        public static Vector2 GetDirectionTo(this Cell from, Cell to, TableMetadata metadata = null)
        {
            if (from == null || to == null)
                return Vector2.zero;
            
            Table commonTable = from.GetNearestCommonTable(to, out from, out to);
            (int col, int row) fromPosition = PositionUtil.GetPosition(from.GetLocalPosition());
            (int col, int row) toPosition = PositionUtil.GetPosition(to.GetLocalPosition());
            
            if (metadata != null && !commonTable.IsSubTable && metadata.IsTransposed)
            {
                fromPosition = (fromPosition.row, fromPosition.col);
                toPosition = (toPosition.row, toPosition.col);
            }

            return new Vector2(Mathf.Clamp(toPosition.col - fromPosition.col, -1, 1), -Mathf.Clamp(toPosition.row - fromPosition.row, -1, 1));
        }
        
        public static List<Vector2Int> GetDistancesByDepth(this Cell to, Cell from)
        {
            if (to == null || from == null)
                return new List<Vector2Int>();

            List<Vector2Int> distances = new List<Vector2Int>();
            List<Cell> fromAncestors = from.GetAncestors(true).ToList();
            List<Cell> toAncestors = to.GetAncestors(true).ToList();

            while (fromAncestors.Count > toAncestors.Count)
                toAncestors.Insert(0, null);
            while (toAncestors.Count > fromAncestors.Count)
                fromAncestors.Insert(0, null);
            
            for(int i = 0; i < fromAncestors.Count; i++)
            {
                Cell fromCell = fromAncestors[i];
                Cell toCell = toAncestors[i];

                Vector2Int fromPosition = fromCell != null ? new Vector2Int(fromCell.column.Position, fromCell.row.Position) : Vector2Int.one;
                Vector2Int toPosition = toCell != null ? new Vector2Int(toCell.column.Position, toCell.row.Position) : Vector2Int.one;

                Vector2Int distance = new Vector2Int(toPosition.x - fromPosition.x, toPosition.y - fromPosition.y);
                distances.Add(distance);
            }

            return distances;
        }
        
        public static void SetFocused(this Cell cell, bool focused)
        {
            CellControl cellControl = CellControlFactory.GetCellControlFromId(cell.Id);
            if (cellControl != null)
            {
                cellControl.SetFocused(focused);
            }
        }
        
        public static void BringToView(this Cell cell, TableControl rootTableControl)
        {
            TableControl tableControl = null;
            if (cell.Table.IsSubTable)
            {
                cell.Table.ParentCell.BringToView(rootTableControl);
                tableControl = (CellControlFactory.GetCellControlFromId(cell.Table.ParentCell.Id) as SubTableCellControl)?.SubTableControl;
            }
            else tableControl = rootTableControl;

            if (tableControl != null)
            {
                int rowId, columnId;
                if (cell.Table == rootTableControl.TableData)
                {
                    rowId = rootTableControl.GetCellRow(cell).Id;
                    columnId = rootTableControl.GetCellColumn(cell).Id;
                }
                else
                {
                    rowId = cell.row.Id;
                    columnId = cell.column.Id;
                }
                        
                RowHeaderControl rowHeader = tableControl.RowHeaders[rowId];
                ColumnHeaderControl columnHeader = tableControl.ColumnHeaders[columnId];

                if (!tableControl.RowVisibilityManager.IsHeaderCompletelyInBounds(rowHeader, false, out var visibleBoundsY))
                {
                    MoveVerticalScroll(visibleBoundsY, tableControl, rowHeader);
                }

                if (!tableControl.ColumnVisibilityManager.IsHeaderCompletelyInBounds(columnHeader, false, out var visibleBoundsX))
                {
                    MoveHorizontalScroll(visibleBoundsX, tableControl, columnHeader);
                }
            }
        }

        private static void MoveHorizontalScroll(sbyte visibleBoundsX, TableControl tableControl, ColumnHeaderControl columnHeader)
        {
            bool isRightVisible = (visibleBoundsX & 2) == 2;
            bool isLeftVisible = (visibleBoundsX & 1) == 1;
            float delta;

            float scrollviewLeft = tableControl.ScrollView.contentViewport.worldBound.xMin + tableControl.CornerContainer.CornerControl.resolvedStyle.width;
            float scrollviewRight = tableControl.ScrollView.contentViewport.worldBound.xMax;

            float widthDiff = columnHeader.worldBound.width - (tableControl.ScrollView.contentViewport.worldBound.width - tableControl.CornerContainer.CornerControl.resolvedStyle.width);
            if(widthDiff >= 0)
                delta = columnHeader.worldBound.xMin + widthDiff / 2 - scrollviewLeft;
            else if (!isLeftVisible && isRightVisible)
                delta = columnHeader.worldBound.xMin - scrollviewLeft;
            else if(!isRightVisible && isLeftVisible)
                delta = columnHeader.worldBound.xMax - scrollviewRight;
            else
            {
                bool isOverViewport = columnHeader.worldBound.xMax >= scrollviewLeft;
                if(isOverViewport)
                    delta = columnHeader.worldBound.xMax - scrollviewRight;
                else
                    delta = columnHeader.worldBound.xMin - scrollviewLeft;
            }
                    
            tableControl.ScrollView.horizontalScroller.value += delta;
            tableControl.AdjustHorizontalScroller();
        }

        private static void MoveVerticalScroll(sbyte visibleBoundsY, TableControl tableControl, RowHeaderControl rowHeader)
        {
            bool isTopVisible = (visibleBoundsY & 2) == 2;
            bool isBottomVisible = (visibleBoundsY & 1) == 1;
            float delta;
                    
            float scrollviewTop = tableControl.ScrollView.contentViewport.worldBound.yMin + tableControl.CornerContainer.CornerControl.resolvedStyle.height;
            float scrollviewBottom = tableControl.ScrollView.contentViewport.worldBound.yMax;

            float heightDiff = rowHeader.worldBound.height - (tableControl.ScrollView.contentViewport.worldBound.height - tableControl.CornerContainer.CornerControl.resolvedStyle.height);
            if(heightDiff >= 0)
                delta = rowHeader.worldBound.yMin + heightDiff / 2 - scrollviewTop;
            else if (!isTopVisible && isBottomVisible)
                delta = rowHeader.worldBound.yMin - scrollviewTop;
            else if(!isBottomVisible && isTopVisible)
                delta = rowHeader.worldBound.yMax - scrollviewBottom;
            else
            {
                bool isOverViewport = rowHeader.worldBound.yMax <= scrollviewTop;
                if(isOverViewport)
                    delta = rowHeader.worldBound.yMin - scrollviewTop;
                else
                    delta = rowHeader.worldBound.yMax - scrollviewBottom;
            }
                    
            tableControl.ScrollView.verticalScroller.value += delta;
            tableControl.AdjustVerticalScroller();
        }
    }
}