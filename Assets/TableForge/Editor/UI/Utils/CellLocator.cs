using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class CellLocator
    {
        public static CellControl GetCell(TableControl tableControl, int rowId, int columnId)
        {
            if (!tableControl.RowHeaders.TryGetValue(rowId, out var rowHeader) 
                || !tableControl.ColumnData.TryGetValue(columnId, out var columnAnchor)) return null;
            
            VisualElement row = rowHeader.RowControl;
            if(row.Children().Count() <= columnAnchor.Position - 1) return null;
            
            return row.Children().ElementAt(columnAnchor.Position - 1) as CellControl;
        }
        
        public static List<CellControl> GetCellRange(TableControl tableControl, int startRowId, int startColumnId, int endRowId, int endColumnId)
        {
            var cells = new List<CellControl>();
            
            int startingRowPosition = tableControl.RowData[startRowId].Position;
            int endingRowPosition = tableControl.RowData[endRowId].Position;

            int startingColumnPosition = tableControl.ColumnData[startColumnId].Position - 1;
            int endingColumnPosition = tableControl.ColumnData[endColumnId].Position - 1;

            if(startingRowPosition > endingRowPosition)
                (startingRowPosition, endingRowPosition) = (endingRowPosition, startingRowPosition);
            if(startingColumnPosition > endingColumnPosition)
                (startingColumnPosition, endingColumnPosition) = (endingColumnPosition, startingColumnPosition);
            
            List<VisualElement> rows = new List<VisualElement>();
            if(startingRowPosition < 0 || endingRowPosition > tableControl.RowHeaders.Count) return new List<CellControl>();

            Table table = tableControl.TableData;
            
            for (int i = startingRowPosition; i <= endingRowPosition; i++)
            {
                rows.Add(tableControl.RowHeaders[table.Rows[i].Id].RowControl);
            }
            
            foreach (var row in rows)
            {
                if(startingColumnPosition < 0 || endingColumnPosition >= row.Children().Count()) return  new List<CellControl>();
                
                for (int i = startingColumnPosition; i <= endingColumnPosition; i++)
                {
                    if(row.Children().ElementAt(i) is CellControl cell)
                        cells.Add(cell);
                }
            }

            return cells;
        }

        public static CellControl GetContiguousCell(TableControl tableControl, int rowId, int columnId, Vector2 direction)
        {
            int rowPosition = tableControl.RowData[rowId].Position - 1;
            int columnPosition = tableControl.ColumnData[columnId].Position - 1;
            
            int newRowPosition = rowPosition + (int)direction.y;
            int newColumnPosition = columnPosition + (int)direction.x;
            
            if (newRowPosition < 0 || newRowPosition >= tableControl.RowHeaders.Count) return null;
            int newRowId = tableControl.TableData.Rows[newRowPosition].Id;
            RowControl row = tableControl.RowHeaders[newRowId].RowControl;
            
            if (newColumnPosition < 0 || newColumnPosition >= row.Children().Count()) return null;
            return row.Children().ElementAt(newColumnPosition) as CellControl;
        }
        
        public static List<CellControl> GetCellsAtRow(TableControl tableControl, int rowId)
        {
            VisualElement row = tableControl.RowHeaders[rowId].RowControl;
            return row.Children().OfType<CellControl>().ToList();
        }
        
        public static List<CellControl> GetCellsAtColumn(TableControl tableControl, int columnId)
        {
            if (!tableControl.ColumnData.TryGetValue(columnId, out var columnAnchor)) return new List<CellControl>();
            return tableControl.RowHeaders.Values.Select
            (r => 
                r.RowControl.Children().Any() ? 
                    r.RowControl.Children().ElementAt(columnAnchor.Position - 1) 
                    : null
            ).OfType<CellControl>().ToList();
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