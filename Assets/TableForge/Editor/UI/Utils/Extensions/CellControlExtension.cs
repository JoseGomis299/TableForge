using System.Collections.Generic;

namespace TableForge.UI
{
    internal static class CellControlExtension
    {
        /// <summary>
        ///  Gets the highest ancestor of a cell in the table hierarchy. If there is not, it returns itself.
        /// </summary>
        public static CellControl GetHighestAncestor(this CellControl cell)
        {
            CellControl currentCell = cell.TableControl.Parent;

            while (currentCell != null)
            {
                if (currentCell.TableControl.Parent == null)
                    return currentCell;
                
                currentCell = currentCell.TableControl.Parent;
            }

            return currentCell;
        }
        
        public static IEnumerable<CellControl> GetAncestors(this CellControl cell, bool includeSelf = false)
        {
            if (includeSelf)
                yield return cell;
            
            CellControl currentCell = cell.TableControl.Parent;

            while (currentCell != null)
            {
                yield return currentCell;
                currentCell = currentCell.TableControl.Parent;
            }
        }
        
        public static void SetFocused(this CellControl cellControl, bool focused)
        {
            if (focused)
            {
                cellControl.focusable = true;
                cellControl.Focus();
                cellControl.AddToClassList(USSClasses.Focused);
                foreach (var ancestor in cellControl.GetAncestors(true))
                {
                    ancestor.LockHeadersVisibility();
                    
                    if(ancestor is SubTableCellControl subTableCellControl)
                    {
                        subTableCellControl.SubTableControl?.ScrollView.SetScrollbarsVisibility(true);
                    }
                }
            }
            else
            {
                cellControl.focusable = false;
                cellControl.RemoveFromClassList(USSClasses.Focused);
                
                var tableControl = cellControl.TableControl;
                RowHeaderControl row = tableControl.GetRowHeaderControl(tableControl.GetCellRow(cellControl.Cell));
                ColumnHeaderControl column = tableControl.GetColumnHeaderControl(tableControl.GetCellColumn(cellControl.Cell));
                
                if(!cellControl.TableControl.RowVisibilityManager.IsHeaderVisibilityLockedBy(row, cellControl.Cell)
                   && !cellControl.TableControl.ColumnVisibilityManager.IsHeaderVisibilityLockedBy(column, cellControl.Cell))
                    return;
                
                foreach (var ancestor in cellControl.GetAncestors(true))
                {
                    ancestor.UnlockHeadersVisibility();
                    
                    if(ancestor is SubTableCellControl subTableCellControl)
                    {
                        subTableCellControl.SubTableControl?.ScrollView.SetScrollbarsVisibility(false);
                    }
                }
            }
        }
        
        private static void UnlockHeadersVisibility(this CellControl cellControl)
        {
            var tableControl = cellControl.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(cellControl.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(cellControl.Cell);
            tableControl.RowVisibilityManager.UnlockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], cellControl.Cell);
            tableControl.ColumnVisibilityManager.UnlockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], cellControl.Cell);
        }

        private static void LockHeadersVisibility(this CellControl cellControl)
        {
            var tableControl = cellControl.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(cellControl.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(cellControl.Cell);
            tableControl.RowVisibilityManager.LockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], cellControl.Cell);
            tableControl.ColumnVisibilityManager.LockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], cellControl.Cell);
        }
    }
}