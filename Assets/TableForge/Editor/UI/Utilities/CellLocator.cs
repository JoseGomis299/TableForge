using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class CellLocator
    {
        public static Cell GetCell(TableControl tableControl, int rowId, int columnId)
        {
            if (!tableControl.RowHeaders.TryGetValue(rowId, out var rowHeader) 
                || !tableControl.ColumnData.TryGetValue(columnId, out var columnAnchor)) return null;

            return tableControl.GetCell(rowHeader.RowControl.Anchor.Id, columnAnchor.Id);
        }
        
        public static List<Cell> GetCellRange(Cell firstCell, Cell lastCell, TableControl rootTableControl)
        {
            var cells = new List<Cell>();
            
            List<Cell> firstCellHierarchy = firstCell.GetAncestors(true).ToList();
            List<Cell> lastCellHierarchy = lastCell.GetAncestors(true).ToList();
            
            GetCellRange(firstCell, lastCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, cells);

            return cells;
        }

        private static void GetCellRange(Cell firstCell, Cell lastCell, List<Cell> firstCellHierarchy, List<Cell> lastCellHierarchy, TableControl rootTableControl, List<Cell> result)
        {
            if(firstCell.GetDepth() > lastCell.GetDepth())
            { 
                result.Add(firstCell);

                Vector2 direction = firstCell.GetDirectionTo(lastCell, rootTableControl?.Metadata);
                Cell last = direction.x switch
                {
                    < 0 => firstCell.Table.GetFirstCell(),
                    > 0 => firstCell.Table.GetLastCell(),
                    _ => direction.y < 0 ? firstCell.Table.GetLastCell() : firstCell.Table.GetFirstCell()
                };

                //Select the corresponding cells in the first cell's table
                GetCellRange(firstCell, last, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                //Select the corresponding cells in the first cell's parent table
                GetCellRange(firstCell.Table.ParentCell, lastCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                
                //Select the corresponding cells in the last cell's table
                result.AddRange(lastCell.GetDescendants(firstCell.GetDepth()));
                return;
            }
            
            if (lastCell.GetDepth() > firstCell.GetDepth())
            {
                result.Add(lastCell);

                Vector2 direction = lastCell.GetDirectionTo(firstCell, rootTableControl?.Metadata);
                Cell first = direction.x switch
                {
                    < 0 => lastCell.Table.GetFirstCell(),
                    > 0 => lastCell.Table.GetLastCell(),
                    _ => direction.y < 0 ? lastCell.Table.GetLastCell() : lastCell.Table.GetFirstCell()
                };

                //Select the corresponding cells in the last cell's table
                GetCellRange(first, lastCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                //Select the corresponding cells in the last cell's parent table
                GetCellRange(firstCell, lastCell.Table.ParentCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                
                //Select the corresponding cells in the first cell's table
                result.AddRange(firstCell.GetDescendants(lastCell.GetDepth()));
                return;
            }

            if (firstCell.Table != lastCell.Table)
            {
                Vector2 direction = firstCell.GetDirectionTo(lastCell, rootTableControl?.Metadata);
                Cell last = direction.x switch
                {
                    < 0 => firstCell.Table.GetFirstCell(),
                    > 0 => firstCell.Table.GetLastCell(),
                    _ => direction.y < 0 ? firstCell.Table.GetLastCell() : firstCell.Table.GetFirstCell()
                };
                
                Cell first = direction.x switch
                {
                    > 0 => lastCell.Table.GetFirstCell(),
                    < 0 => lastCell.Table.GetLastCell(),
                    _ => direction.y >= 0 ? lastCell.Table.GetLastCell() : lastCell.Table.GetFirstCell()
                };

                //Select the corresponding cells in the first cell's table
                GetCellRange(firstCell, last, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                //Select the corresponding cells in the last cell's table
                GetCellRange(first, lastCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                
                //Select the corresponding cells in the common parent table
                GetCellRange(firstCell.Table.ParentCell, lastCell.Table.ParentCell, firstCellHierarchy, lastCellHierarchy, rootTableControl, result);
                return;
            }

            if (firstCell.Table == lastCell.Table)
            {
                //Select the ancestor cells if selecting inside a subtable
                result.AddRange(firstCell.GetAncestors());
            }

            Table table = firstCell.Table;
            bool isMainTable = table == rootTableControl?.TableData;
            bool isTransposed = isMainTable && (rootTableControl?.Transposed ?? false);

            int startingRowPosition = isTransposed ? firstCell.column.Position : firstCell.row.Position;
            int endingRowPosition = isTransposed ? lastCell.column.Position : lastCell.row.Position;

            int startingColumnPosition = isTransposed ? firstCell.row.Position : firstCell.column.Position;
            int endingColumnPosition = isTransposed ? lastCell.row.Position : lastCell.column.Position;
            
            if(startingRowPosition > endingRowPosition)
                (startingRowPosition, endingRowPosition) = (endingRowPosition, startingRowPosition);
            if(startingColumnPosition > endingColumnPosition)
                (startingColumnPosition, endingColumnPosition) = (endingColumnPosition, startingColumnPosition);
            
            if(isTransposed)
            {
                (startingRowPosition, startingColumnPosition) = (startingColumnPosition, startingRowPosition);
                (endingRowPosition, endingColumnPosition) = (endingColumnPosition, endingRowPosition);
            }
            
            List<Row> rows = new List<Row>();
            for (int i = startingRowPosition; i <= endingRowPosition; i++)
            {
                rows.Add(table.Rows[i]);
            }
            
            foreach (var row in rows)
            {
                if(rootTableControl != null && !rootTableControl.Filterer.IsVisible(row.GetRootAnchor().Id))
                    continue;
                
                for (int i = startingColumnPosition; i <= endingColumnPosition; i++)
                {
                    if(rootTableControl != null && !rootTableControl.Metadata.IsFieldVisible(row.Table.Columns[i].GetRootAnchor().Id))
                        continue;
                    
                    result.Add(row.Cells[i]);

                    if (row.Cells[i] is SubTableCell subTableCell)
                    {
                        if(firstCellHierarchy.Contains(subTableCell))
                            continue;
                        
                        if(lastCellHierarchy.Contains(subTableCell))
                            continue;
                        
                        result.AddRange(subTableCell.GetDescendants(Mathf.Max(firstCellHierarchy.Count - 1, lastCellHierarchy.Count - 1)));
                    }
                }
            }
        }
        
        public static Cell GetContiguousCell(Cell currentCell, Vector2 direction, Vector2 wrappingMinBounds, Vector2 wrappingMaxBounds)
        {
            int rowPosition = currentCell.row.Position;
            int columnPosition = currentCell.column.Position;
            
            int newRowPosition = rowPosition - (int)direction.y;
            int newColumnPosition = columnPosition + (int)direction.x;

            bool rowChanged = false;
            if (newRowPosition < wrappingMinBounds.y)
            {
                newRowPosition = (int)wrappingMaxBounds.y;
                newColumnPosition--;
                rowChanged = true;
            }
            else if (newRowPosition > wrappingMaxBounds.y)
            {
                newRowPosition = (int)wrappingMinBounds.y;
                newColumnPosition++;
                rowChanged = true;
            }
            
            if (newColumnPosition < wrappingMinBounds.x)
            {
                newColumnPosition = (int)wrappingMaxBounds.x;
                if (!rowChanged)
                {
                    newRowPosition--;

                    if (newRowPosition < wrappingMinBounds.y)
                    {
                        newRowPosition = (int)wrappingMaxBounds.y;
                    }
                }
            }
            else if (newColumnPosition > wrappingMaxBounds.x)
            {
                newColumnPosition = (int)wrappingMinBounds.x;
                if(!rowChanged)
                {
                    newRowPosition++;

                    if (newRowPosition > wrappingMaxBounds.y)
                    {
                        newRowPosition = (int)wrappingMinBounds.y;
                    }
                }
            }

            return currentCell.Table.GetCell(newColumnPosition, newRowPosition);
        }
        
        public static IReadOnlyList<Cell> GetCellsAtRow(TableControl tableControl, int rowId)
        {
            CellAnchor anchor = tableControl.RowData[rowId];
            if (anchor is Row row)
            {
                List<Cell> cells = new List<Cell>();
                foreach (var cell in row.OrderedCells)
                {
                    if(!tableControl.Metadata.IsFieldVisible(cell.column.GetRootAnchor().Id)) continue;
                    cells.Add(cell);
                }

                return cells;
            }
            
            return GetCellsAtColumn(tableControl, anchor);
        }
        
        public static IReadOnlyList<Cell> GetCellsAtColumn(TableControl tableControl, int columnId)
        {
            CellAnchor anchor = tableControl.ColumnData[columnId];
            if (anchor is Row row)
            {
                List<Cell> cells = new List<Cell>();
                foreach (var cell in row.OrderedCells)
                {
                    if(!tableControl.Metadata.IsFieldVisible(cell.column.GetRootAnchor().Id)) continue;
                    cells.Add(cell);
                }

                return cells;
            }
            
            return GetCellsAtColumn(tableControl, anchor);
        }

        private static List<Cell> GetCellsAtColumn(TableControl tableControl, CellAnchor column)
        {
            int columnPosition = column.Position;
            return column.Table.OrderedRows.Where(r => tableControl.Filterer.IsVisible(r.GetRootAnchor().Id)).Select(r => r.Cells[columnPosition]).ToList();
        }

        public static (RowHeaderControl row, ColumnHeaderControl column) GetHeadersAtPosition(TableControl tableControl, Vector3 position)
        {
            if(tableControl.CornerContainer.worldBound.Contains(position))
                return (null, null);
            
            var rowHeader = tableControl.RowHeaders.FirstOrDefault(r => r.Value.worldBound.yMax >= position.y && r.Value.worldBound.yMin <= position.y).Value;
            if(rowHeader != null && rowHeader.worldBound.Contains(position))
            {
                return (rowHeader, null);
            }
            
            var columnHeader = tableControl.ColumnHeaders.FirstOrDefault(c => c.Value.worldBound.xMax >= position.x && c.Value.worldBound.xMin <= position.x).Value;
            if(columnHeader != null && columnHeader.worldBound.Contains(position))
            {
                return (null, columnHeader);
            }

            return (rowHeader, columnHeader);
        }
    }
}