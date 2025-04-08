using System.Collections.Generic;
using System.Linq;


namespace TableForge
{
    internal static class CellExtension
    {
        
        /// <summary>
        /// Gets the ascendants of a cell in the table hierarchy. (not including itself)
        /// </summary>
        /// <returns></returns>
        public static List<Cell> GetAncestors(this Cell cell, bool includeSelf = false)
        {
            List<Cell> ancestors = new List<Cell>();
            if(includeSelf)
                ancestors.Add(cell);
            
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                ancestors.Add(currentCell);
                currentCell = currentCell.Table.ParentCell;
            }

            return ancestors;
        }
        
        /// <summary>
        /// Gets the descendants of a cell in the table hierarchy. (not including itself)
        /// </summary>
        /// <returns></returns>
        public static List<Cell> GetDescendants(this Cell cell, int maxDepth = -1)
        {
            List<Cell> descendants = new List<Cell>();
            int currentDepth = cell.GetDepth();
            if(currentDepth >= maxDepth && maxDepth != -1)
                return descendants;
            
            if(cell is SubTableCell subTableCell)
            {
                foreach (var row in subTableCell.SubTable.Rows.Values)
                {
                    foreach (var descendantCell in row.Cells.Values)
                    {
                        if (descendantCell is SubTableCell subTableCellDescendant)
                        {
                            if (maxDepth == -1 || currentDepth + 1 < maxDepth)
                                descendants.AddRange(subTableCellDescendant.GetDescendants(maxDepth));
                        }
                        
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

            return currentCell ?? cell;
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
        /// Get the Table which contains the two nearest ancestors of the two cells.
        /// </summary>
        public static Table GetNearestCommonTable(this Cell cell1, Cell cell2, out Cell cell1Ancestor, out Cell cell2Ancestor)
        {
            Dictionary<Table, Cell> cell1Ancestors = cell1.GetAncestors(true).Select(x => new KeyValuePair<Table, Cell>(x.Table, x)).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<Table, Cell> cell2Ancestors = cell2.GetAncestors(true).Select(x => new KeyValuePair<Table, Cell>(x.Table, x)).ToDictionary(x => x.Key, x => x.Value);
            
            foreach (var ancestor in cell1Ancestors)
            {
                Table table = ancestor.Key;
                if (cell2Ancestors.ContainsKey(table))
                {
                    cell1Ancestor = cell1Ancestors[table];
                    cell2Ancestor = cell2Ancestors[table];
                    return table;
                }
            }
            
            cell1Ancestor = null;
            cell2Ancestor = null;
            return null;
        }
        
        /// <summary>
        /// Gets the depth of a cell in the table hierarchy. The depth is defined as the number of ascendants of the cell.
        /// </summary>
        public static int GetDepth(this Cell cell)
        {
            int depth = 0;
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                depth++;
                currentCell = currentCell.Table.ParentCell;
            }

            return depth;
        }
    }
}