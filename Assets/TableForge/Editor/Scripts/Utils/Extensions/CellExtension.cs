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
        public static IEnumerable<Cell> GetAncestors(this Cell cell, bool includeSelf = false)
        {
            if(includeSelf)
                yield return cell;
            
            Cell currentCell = cell.Table.ParentCell;

            while (currentCell != null)
            {
                yield return currentCell;
                currentCell = currentCell.Table.ParentCell;
            }
        }
        
        /// <summary>
        /// Gets the descendants of a cell in the table hierarchy. 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Cell> GetDescendants(this Cell cell, int maxDepth = -1, bool includeSelf = false)
        {
            int currentDepth = cell.GetDepth();
            if(includeSelf) yield return cell;
            if(currentDepth >= maxDepth && maxDepth != -1)
                yield break;
            
            if(cell is SubTableCell subTableCell)
            {
                foreach (var row in subTableCell.SubTable.OrderedRows)
                {
                    foreach (var descendantCell in row.OrderedCells)
                    {
                        yield return descendantCell;

                        if (descendantCell is SubTableCell subTableCellDescendant)
                        {
                            if (maxDepth == -1 || currentDepth + 1 < maxDepth)
                            {
                                foreach (var descendant in subTableCellDescendant.GetDescendants(maxDepth))
                                {
                                    yield return descendant;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the count of all descendants of a cell in the table hierarchy.
        /// </summary>
        /// <param name="cell">The parent cell</param>
        /// <param name="countNulls">Whether the count should include null subTable columns</param>
        /// <param name="countSubTables">Whether subTable cells should be counted or just its content</param>
        /// <returns></returns>
        public static int GetDescendantCount(this SubTableCell cell, bool countNulls, bool countSubTables)
        {
            int count = 0;
            
            if(cell.SubTable.Rows.Count == 0)
            {
                return !countNulls ? 0 : cell.GetSubTableColumnCount(true);
            }
            
            foreach (var row in cell.SubTable.OrderedRows)
            {
                foreach (var descendantCell in row.OrderedCells)
                {
                    if (descendantCell is SubTableCell subTableCellDescendant and not ICollectionCell)
                    {
                        if (subTableCellDescendant.SubTable.Rows.Count == 0)
                        {
                            if(!countNulls) continue;
                            count += subTableCellDescendant.GetSubTableColumnCount(true);
                        }
                        else count += subTableCellDescendant.GetDescendantCount(countNulls, countSubTables);

                        if (countSubTables) count++;
                    }
                    else count++;
                    
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// Retrieves the immediate descendants of a cell in the table hierarchy.
        /// </summary>
        public static IEnumerable<Cell> GetImmediateDescendants(this Cell cell)
        {
            if (cell is SubTableCell subTableCell)
            {
                foreach (var row in subTableCell.SubTable.OrderedRows)
                {
                    foreach (var descendantCell in row.OrderedCells)
                    {
                        yield return descendantCell;
                    }
                }
            }
        }
        
        /// <summary>
        /// Determines if a cell is a descendant of another cell in the table hierarchy.
        /// </summary>
        public static bool IsDescendantOf(this Cell cell, Cell childCell)
        {
            foreach (var ancestor in cell.GetAncestors())
            {
                if(ancestor == childCell)
                    return true;
            }
         
            return false;
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
            List<Cell> cell2Ancestors = cell2.GetAncestors().ToList();

            foreach (var ancestor in  cell1.GetAncestors())
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
            
            foreach (var ancestor in cell2.GetAncestors(true))
            {
                Table table = ancestor.Table;
                if (cell1Ancestors.TryGetValue(table, out var ancestor1))
                {
                    cell1Ancestor = ancestor1;
                    cell2Ancestor = ancestor;
                    return table;
                }
            }
            
            cell1Ancestor = null;
            cell2Ancestor = null;
            return null;
        }
        
        /// <summary>
        /// Gets the nearest row in the hierarchy that contains these cells or ancestors.
        /// </summary>
        public static Row GetNearestCommonRow(this Cell cell1, Cell cell2)
        {
            GetNearestCommonTable(cell1, cell2, out var cell1Ancestor, out var cell2Ancestor);
            if (cell1Ancestor.Row != cell2Ancestor.Row)
                return null;

            return cell1Ancestor.Row;
        }
        
        /// <summary>
        /// Gets the nearest column in the hierarchy that contains these cells or ancestors.
        /// </summary>
        public static Column GetNearestCommonColumn(this Cell cell1, Cell cell2)
        {
            GetNearestCommonTable(cell1, cell2, out var cell1Ancestor, out var cell2Ancestor);
            if (cell1Ancestor.Column != cell2Ancestor.Column)
                return null;

            return cell1Ancestor.Column;
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
        
        public static int GetSubTableColumnCount(this SubTableCell cell, bool includeDescendantColumns = false)
        {
            Table table = cell.SubTable;
            if (table == null)
                return 0;
            
            int columnCount = table.Columns.Count;
            if (includeDescendantColumns)
            {
                columnCount = 0;
                var fields = SerializationUtil.GetSerializableFields(cell.Type, null);
                foreach (var field in fields)
                {
                    if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, field.Type, out _))
                        columnCount++;
                    else if (!field.Type.IsSimpleType())
                        columnCount += GetSubTableColumnCount(field);
                }
            }
            
            return columnCount;
        }

        private static int GetSubTableColumnCount(TFFieldInfo field)
        {
            int count = 0;
            var fields = SerializationUtil.GetSerializableFields(field.Type, field.FieldInfo);
            foreach (var subField in fields)
            {
                if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, subField.Type, out _))
                    count++;
                else if (!subField.Type.IsSimpleType())
                    count += GetSubTableColumnCount(subField);
            }
            
            return count;
        }
        
        public static List<Cell> GetKeys(this DictionaryCell cell)
        {
            List<Cell> keys = new List<Cell>();
            foreach (var keyRow in cell.SubTable.OrderedRows)
            {
                if (keyRow == null)
                    continue;

                keys.Add(keyRow.Cells[1]);
            }

            return keys;
        }
        
        public static List<Cell> GetValues(this DictionaryCell cell)
        {
            List<Cell> values = new List<Cell>();
            foreach (var valueRow in cell.SubTable.OrderedRows)
            {
                if (valueRow == null)
                    continue;

                values.Add(valueRow.Cells[2]);
            }

            return values;
        }
    }
}