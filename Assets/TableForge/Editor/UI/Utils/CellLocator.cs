using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.UI
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
            while (firstCell.GetLevel() >  lastCell.GetLevel())
            {
                firstCell = firstCell.Table.ParentCell;
            }
            
            List<Cell> firstCellAncestors = firstCell.GetAncestors();
            List<Cell> lastCellAncestors = lastCell.GetAncestors();
            firstCellAncestors.Insert(0, firstCell);
            lastCellAncestors.Insert(0, lastCell);
            
            Cell firstCellFurthestAncestor = firstCellAncestors[^1];
            Cell lastCellFurthestAncestor = lastCellAncestors[^1];
            
            firstCellAncestors.RemoveAt(firstCellAncestors.Count - 1);
            lastCellAncestors.RemoveAt(lastCellAncestors.Count - 1);
            
            GetCellRange(firstCellFurthestAncestor, lastCellFurthestAncestor, firstCellAncestors, lastCellAncestors, cells, rootTableControl, true);

            return cells;
        }

        private static void GetCellRange(Cell firstCell, Cell lastCell, List<Cell> firstCellAncestors, List<Cell> lastCellAncestors, List<Cell> result, TableControl rootTableControl, bool isRightmost)
        {
            Cell firstCellFurthestAncestor = null;
            Cell lastCellFurthestAncestor = null;

            if (firstCellAncestors.Any())
            {
                firstCellFurthestAncestor = firstCellAncestors.Last();
                firstCellAncestors.RemoveAt(firstCellAncestors.Count - 1);
            }

            if (lastCellAncestors.Any())
            {
                lastCellFurthestAncestor = lastCellAncestors.Last();
                lastCellAncestors.RemoveAt(lastCellAncestors.Count - 1);
            }

            Table table = firstCell.Table;
            bool isMainTable = table == rootTableControl.TableData;
            bool isInverted = isMainTable && rootTableControl.Inverted;

            int startingRowPosition = isInverted ? firstCell.Column.Position : firstCell.Row.Position;
            int endingRowPosition = isInverted ? lastCell.Column.Position : lastCell.Row.Position;

            int startingColumnPosition = isInverted ? firstCell.Row.Position : firstCell.Column.Position;
            int endingColumnPosition = isInverted ? lastCell.Row.Position : lastCell.Column.Position;

            if(startingRowPosition > endingRowPosition)
                (startingRowPosition, endingRowPosition) = (endingRowPosition, startingRowPosition);
            if(startingColumnPosition > endingColumnPosition)
                (startingColumnPosition, endingColumnPosition) = (endingColumnPosition, startingColumnPosition);
            
            if(isInverted)
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
                for (int i = startingColumnPosition; i <= endingColumnPosition; i++)
                {
                    result.Add(row.Cells[i]);
                    
                    if(row.Cells[i] is SubTableCell subTableCell)
                    {
                        bool isLocalRightmost = rootTableControl.Inverted ?
                            row.Position == endingRowPosition 
                            : i == endingColumnPosition;
                        
                        string firstCellPosition = firstCellFurthestAncestor != null ? 
                            firstCellFurthestAncestor.GetPosition() :
                            "A1";

                        string lastCellPosition = 
                            $"{PositionUtil.ConvertToLetters(subTableCell.SubTable.Columns.Count)}{subTableCell.SubTable.Rows.Count}";

                        if (isLocalRightmost && lastCellFurthestAncestor != null)
                        {
                            var positionTuple = PositionUtil.GetPosition(lastCellFurthestAncestor.GetPosition());
                        
                            positionTuple.column = Mathf.Clamp(positionTuple.column, 1, subTableCell.SubTable.Columns.Count);
                            positionTuple.row = Mathf.Clamp(positionTuple.row, 1, subTableCell.SubTable.Rows.Count);
                        
                            lastCellPosition = PositionUtil.ConvertToLetters(positionTuple.column) + positionTuple.row;
                        }
                        
                        Table subTable = subTableCell.SubTable;
                        Cell subTableFirstCell = subTable.GetCell(firstCellPosition);
                        Cell subTableLastCell = subTable.GetCell(lastCellPosition);
                        if (subTableFirstCell != null && subTableLastCell != null)
                        {
                            GetCellRange(subTableFirstCell, subTableLastCell, firstCellAncestors.ToList(), lastCellAncestors.ToList(), result, rootTableControl, isLocalRightmost);
                        }
                    }
                }
            }
        }
        
        public static Cell GetContiguousCell(Cell currentCell, Vector2 direction, Vector2 wrappingMinBounds, Vector2 wrappingMaxBounds)
        {
            int rowPosition = currentCell.Row.Position;
            int columnPosition = currentCell.Column.Position;
            
            int newRowPosition = rowPosition - (int)direction.y;
            int newColumnPosition = columnPosition + (int)direction.x;
            
            if (newRowPosition < wrappingMinBounds.y)
            {
                newRowPosition = (int)wrappingMaxBounds.y;
                newColumnPosition--;
            }
            else if (newRowPosition > wrappingMaxBounds.y)
            {
                newRowPosition = (int)wrappingMinBounds.y;
                newColumnPosition++;
            }
            
            if (newColumnPosition < wrappingMinBounds.x)
            {
                newColumnPosition = (int)wrappingMaxBounds.x;
                newRowPosition--;
                
                if(newRowPosition < wrappingMinBounds.y)
                {
                    newRowPosition = (int)wrappingMaxBounds.y;
                }
            }
            else if (newColumnPosition > wrappingMaxBounds.x)
            {
                newColumnPosition = (int)wrappingMinBounds.x;
                newRowPosition++;
                
                if(newRowPosition > wrappingMaxBounds.y)
                {
                    newRowPosition = (int)wrappingMinBounds.y;
                }
            }

            return currentCell.Table.GetCell($"{PositionUtil.ConvertToLetters(newColumnPosition)}{newRowPosition}");
        }
        
        public static List<Cell> GetCellsAtRow(TableControl tableControl, int rowId)
        {
            CellAnchorData anchorData = tableControl.RowData[rowId];
            if(anchorData.CellAnchor is Row row)
                return GetCellsAtRow(row);
            
            return GetCellsAtColumn(anchorData.CellAnchor);
        }
        
        public static List<Cell> GetCellsAtColumn(TableControl tableControl, int columnId)
        {
            CellAnchorData anchorData = tableControl.ColumnData[columnId];
            if(anchorData.CellAnchor is Row row)
                return GetCellsAtRow(row);
            
            return GetCellsAtColumn(anchorData.CellAnchor);
        }
        
        private static List<Cell> GetCellsAtRow(Row row)
        {
            return row.Cells.Values.ToList();
        }
        
        private static List<Cell> GetCellsAtColumn(CellAnchor column)
        {
            int columnPosition = column.Position;
            return column.Table.Rows.Values.Select(r => r.Cells[columnPosition]).ToList();
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