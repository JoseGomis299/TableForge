using UnityEngine;

namespace TableForge.UI
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
            (int col, int row) fromPosition = PositionUtil.GetPosition(from.GetPosition());
            (int col, int row) toPosition = PositionUtil.GetPosition(to.GetPosition());
            
            if (metadata != null && !commonTable.IsSubTable && metadata.IsTransposed)
            {
                fromPosition = (fromPosition.row, fromPosition.col);
                toPosition = (toPosition.row, toPosition.col);
            }

            return new Vector2(Mathf.Clamp(toPosition.col - fromPosition.col, -1, 1), -Mathf.Clamp(toPosition.row - fromPosition.row, -1, 1));
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
                    rowId = cell.Row.Id;
                    columnId = cell.Column.Id;
                }
                        
                RowHeaderControl rowHeader = tableControl.RowHeaders[rowId];
                ColumnHeaderControl columnHeader = tableControl.ColumnHeaders[columnId];

                if (!tableControl.RowVisibilityManager.IsHeaderCompletelyInBounds(rowHeader, false, out var deltaY))
                    tableControl.ScrollView.verticalScroller.value += deltaY;

                if (!tableControl.ColumnVisibilityManager.IsHeaderCompletelyInBounds(columnHeader, false, out var deltaX))
                    tableControl.ScrollView.horizontalScroller.value += deltaX;
            }
        }
    }
}