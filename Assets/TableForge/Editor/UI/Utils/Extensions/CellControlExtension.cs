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
                }
            }
            else
            {
                cellControl.focusable = false;
                cellControl.RemoveFromClassList(USSClasses.Focused);
                foreach (var ancestor in cellControl.GetAncestors(true))
                {
                    ancestor.UnlockHeadersVisibility();
                }
            }
        }
        
        private static void UnlockHeadersVisibility(this CellControl cellControl)
        {
            var tableControl = cellControl.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(cellControl.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(cellControl.Cell);
            tableControl.RowVisibilityManager.UnlockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], tableControl.CellSelector);
            tableControl.ColumnVisibilityManager.UnlockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], tableControl.CellSelector);
        }

        private static void LockHeadersVisibility(this CellControl cellControl)
        {
            var tableControl = cellControl.TableControl;
            CellAnchor ancestorRow = tableControl.GetCellRow(cellControl.Cell);
            CellAnchor ancestorColumn = tableControl.GetCellColumn(cellControl.Cell);
            tableControl.RowVisibilityManager.LockHeaderVisibility(tableControl.RowHeaders[ancestorRow.Id], tableControl.CellSelector);
            tableControl.ColumnVisibilityManager.LockHeaderVisibility(tableControl.ColumnHeaders[ancestorColumn.Id], tableControl.CellSelector);
        }
    }
}