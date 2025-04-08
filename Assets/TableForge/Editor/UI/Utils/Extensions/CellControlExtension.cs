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
    }
}