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

            int rowPos = rowHeader.RowControl.Anchor.Position;
            int columnPos = columnAnchor.Position;
            
            return tableControl.TableData.Rows[rowPos].Cells[columnPos];
        }
        
        public static List<Cell> GetCellRange(TableControl tableControl, int startRowId, int startColumnId, int endRowId, int endColumnId)
        {
            var cells = new List<Cell>();
            
            int startingRowPosition = tableControl.RowData[startRowId].Position;
            int endingRowPosition = tableControl.RowData[endRowId].Position;

            int startingColumnPosition = tableControl.ColumnData[startColumnId].Position;
            int endingColumnPosition = tableControl.ColumnData[endColumnId].Position;

            if(startingRowPosition > endingRowPosition)
                (startingRowPosition, endingRowPosition) = (endingRowPosition, startingRowPosition);
            if(startingColumnPosition > endingColumnPosition)
                (startingColumnPosition, endingColumnPosition) = (endingColumnPosition, startingColumnPosition);
            
            List<Row> rows = new List<Row>();
            if(startingRowPosition < 0 || endingRowPosition > tableControl.RowHeaders.Count) return new List<Cell>();

            Table table = tableControl.TableData;
            
            for (int i = startingRowPosition; i <= endingRowPosition; i++)
            {
                rows.Add(table.Rows[i]);
            }
            
            foreach (var row in rows)
            {
                if(startingColumnPosition < 0 || endingColumnPosition >= row.Cells.Count) return  new List<Cell>();
                
                for (int i = startingColumnPosition; i <= endingColumnPosition; i++)
                {
                    cells.Add(row.Cells[i]);
                }
            }

            return cells;
        }

        public static Cell GetContiguousCell(TableControl tableControl, int rowId, int columnId, Vector2 direction)
        {
            int rowPosition = tableControl.RowData[rowId].Position - 1;
            int columnPosition = tableControl.ColumnData[columnId].Position - 1;
            
            int newRowPosition = rowPosition + (int)direction.y;
            int newColumnPosition = columnPosition + (int)direction.x;
            
            if (newRowPosition < 0 || newRowPosition >= tableControl.TableData.Rows.Count) return null;
            
            return tableControl.TableData.Rows[newRowPosition].Cells[newColumnPosition]; 
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