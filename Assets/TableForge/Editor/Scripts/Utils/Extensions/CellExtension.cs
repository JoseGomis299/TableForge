using System.Collections.Generic;

namespace TableForge
{
    internal static class CellExtension
    {
        
        /// <summary>
        /// Gets the ascendants of a cell in the table hierarchy. (not including itself)
        /// </summary>
        /// <returns></returns>
        public static List<Cell> GetAncestors(this Cell cell)
        {
            List<Cell> ascendants = new List<Cell>();
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                ascendants.Add(currentCell);
                currentCell = currentCell.Table.ParentCell;
            }

            return ascendants;
        }
        
        /// <summary>
        /// Gets the descendants of a cell in the table hierarchy. (not including itself)
        /// </summary>
        /// <returns></returns>
        public static List<Cell> GetDescendants(this Cell cell)
        {
            List<Cell> descendants = new List<Cell>();
            
            if(cell is SubTableCell subTableCell)
            {
                foreach (var row in subTableCell.SubTable.Rows.Values)
                {
                    foreach (var descendantCell in row.Cells.Values)
                    {
                        if(descendantCell is SubTableCell subTableCellDescendant)
                            descendants.AddRange(subTableCellDescendant.GetDescendants());
                        
                        descendants.Add(descendantCell);
                    }
                }
            }

            return descendants;
        }
        
        /// <summary>
        ///  Gets the highest ancestor of a cell in the table hierarchy. If there is not, it returns itself.
        /// </summary>
        public static Cell GetHighestAncestor(this Cell cell)
        {
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                if (currentCell.Table.ParentCell == null)
                    return currentCell;
                
                currentCell = currentCell.Table.ParentCell;
            }

            return currentCell;
        }
        
        
        /// <summary>
        /// Get the common ancestor of two cells in the table hierarchy that is the nearest to the cells.
        /// </summary>
        public static Cell GetNearestCommonAncestor(this Cell cell1, Cell cell2)
        {
            List<Cell> cell1Ancestors = cell1.GetAncestors();
            List<Cell> cell2Ancestors = cell2.GetAncestors();

            foreach (var ancestor in cell1Ancestors)
            {
                if (cell2Ancestors.Contains(ancestor))
                    return ancestor;
            }

            return null;
        }
        
        /// <summary>
        /// Gets the level of a cell in the table hierarchy. The level is defined as the number of ascendants of the cell.
        /// </summary>
        public static int GetLevel(this Cell cell)
        {
            int level = 0;
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                level++;
                currentCell = currentCell.Table.ParentCell;
            }

            return level;
        }
    }
}