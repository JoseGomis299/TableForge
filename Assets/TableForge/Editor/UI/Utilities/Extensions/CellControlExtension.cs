using System.Collections.Generic;
using TableForge.Editor.UI.UssClasses;

namespace TableForge.Editor.UI
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
        
        public static void SetFocused(this CellControl cellControl, bool focus)
        {
            if (focus && !cellControl.focusable) //Set focus
            {
                cellControl.focusable = true;
                cellControl.Focus();
                cellControl.LowerOverlay.AddToClassList(TableVisualizerUss.Focused);
                foreach (var ancestor in cellControl.GetAncestors(true))
                {
                    ancestor.LockHeadersVisibility();
                    
                    if(ancestor is SubTableCellControl subTableCellControl)
                    {
                        subTableCellControl.SubTableControl?.SetScrollbarsVisibility(true);
                        if (subTableCellControl is ExpandableSubTableCellControl expandableSubTableCellControl)
                        {
                            expandableSubTableCellControl.ShowToolbar(true, false);
                        }
                    }
                }
            }
            else if(!focus && cellControl.focusable) //Remove focus
            {
                cellControl.focusable = false;
                cellControl.LowerOverlay.RemoveFromClassList(TableVisualizerUss.Focused);
                
                var tableControl = cellControl.TableControl;
                RowHeaderControl row = tableControl.GetRowHeaderControl(tableControl.GetCellRow(cellControl.Cell));
                ColumnHeaderControl column = tableControl.GetColumnHeaderControl(tableControl.GetCellColumn(cellControl.Cell));
                
                bool headerIsLockedByCell = tableControl.RowVisibilityManager.IsHeaderVisibilityLockedBy(row, cellControl.Cell)
                                           || tableControl.ColumnVisibilityManager.IsHeaderVisibilityLockedBy(column, cellControl.Cell);
                
                foreach (var ancestor in cellControl.GetAncestors(true))
                {
                    if(headerIsLockedByCell)
                        ancestor.UnlockHeadersVisibility();
                    
                    if(ancestor is SubTableCellControl subTableCellControl)
                    {
                        subTableCellControl.SubTableControl?.SetScrollbarsVisibility(false);
                        if (subTableCellControl is ExpandableSubTableCellControl expandableSubTableCellControl)
                        {
                            expandableSubTableCellControl.ShowToolbar(false, false);
                        }
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