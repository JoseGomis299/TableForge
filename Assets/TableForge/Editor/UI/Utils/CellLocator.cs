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
            if (!tableControl.RowData.TryGetValue(rowId, out var rowAnchor) || !tableControl.ColumnData.TryGetValue(columnId, out var columnAnchor)) return null;
            //We subtract 1 from the row position because internally we start at 1, but the visual element children collection starts at 0
            if(tableControl.RowsContainer.Children().Count() <= rowAnchor.Position - 1) return null;
            
            VisualElement row = tableControl.RowsContainer.Children().ElementAt(rowAnchor.Position - 1);
            //We don't subtract 1 from the column position because the row header is the first child of the row
            if(row.Children().Count() <= columnAnchor.Position) return null;
            
            return row.Children().ElementAt(columnAnchor.Position) as CellControl;
        }
        
        public static List<CellControl> GetCellRange(TableControl tableControl, int startRowId, int startColumnId, int endRowId, int endColumnId)
        {
            var cells = new List<CellControl>();
            
            int startingRowPosition = tableControl.RowData[startRowId].Position - 1;
            int endingRowPosition = tableControl.RowData[endRowId].Position - 1;

            int startingColumnPosition = tableControl.ColumnData[startColumnId].Position;
            int endingColumnPosition = tableControl.ColumnData[endColumnId].Position;

            if(startingRowPosition > endingRowPosition)
                (startingRowPosition, endingRowPosition) = (endingRowPosition, startingRowPosition);
            if(startingColumnPosition > endingColumnPosition)
                (startingColumnPosition, endingColumnPosition) = (endingColumnPosition, startingColumnPosition);
            
            List<VisualElement> rows = new List<VisualElement>();
            if(startingRowPosition < 0 || endingRowPosition >= tableControl.RowsContainer.Children().Count()) return new List<CellControl>();

            
            for (int i = startingRowPosition; i <= endingRowPosition; i++)
            {
                rows.Add(tableControl.RowsContainer.Children().ElementAt(i));
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
            int columnPosition = tableControl.ColumnData[columnId].Position;
            
            int newRowPosition = rowPosition + (int)direction.y;
            int newColumnPosition = columnPosition + (int)direction.x;
            
            if (newRowPosition < 0 || newRowPosition >= tableControl.RowsContainer.Children().Count()) return null;
            VisualElement row = tableControl.RowsContainer.Children().ElementAt(newRowPosition);
            
            if (newColumnPosition < 0 || newColumnPosition >= row.Children().Count()) return null;
            return row.Children().ElementAt(newColumnPosition) as CellControl;
        }
        
        public static List<CellControl> GetCellsAtRow(TableControl tableControl, int rowId)
        {
            if (!tableControl.RowData.TryGetValue(rowId, out var rowAnchor)) return null;
            //We subtract 1 from the row position because internally we start at 1, but the visual element children collection starts at 0
            if(tableControl.RowsContainer.Children().Count() <= rowAnchor.Position - 1) return null;
            
            VisualElement row = tableControl.RowsContainer.Children().ElementAt(rowAnchor.Position - 1);
            return row.Children().OfType<CellControl>().ToList();
        }
        
        public static List<CellControl> GetCellsAtColumn(TableControl tableControl, int columnId)
        {
            if (!tableControl.ColumnData.TryGetValue(columnId, out var columnAnchor)) return new List<CellControl>();
            return tableControl.RowsContainer.Children().Select(r => r.Children().ElementAt(columnAnchor.Position)).OfType<CellControl>().ToList();
        }

        public static (RowHeaderControl row, ColumnHeaderControl column) GetHeadersAtPosition(TableControl tableControl, Vector3 position)
        {
            var rowHeader = tableControl.RowHeaders.FirstOrDefault(r => r.Value.worldBound.yMax >= position.y && r.Value.worldBound.yMin <= position.y).Value;
            if(rowHeader != null && rowHeader.worldBound.Contains(position))
            {
                if(tableControl.HorizontalResizer.IsResizingArea(position, out _))
                    return (null, null);
                return (rowHeader, null);
            }
            
            var columnHeader = tableControl.ColumnHeaders.FirstOrDefault(c => c.Value.worldBound.xMax >= position.x && c.Value.worldBound.xMin <= position.x).Value;
            if(columnHeader != null && columnHeader.worldBound.Contains(position))
            {
                if(tableControl.VerticalResizer.IsResizingArea(position, out _))
                    return (null, null);
                return (null, columnHeader);
            }

            return (rowHeader, columnHeader);
        }
    }
}