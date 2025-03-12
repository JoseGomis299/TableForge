using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class RowControl : VisualElement
    {
        private readonly Dictionary<int, CellControl> _cells = new Dictionary<int, CellControl>();
        private TableControl TableControl { get; }
        
        public RowControl(CellAnchor anchor, TableControl tableControl)
        {
            TableControl = tableControl;
            
            if (anchor is Row row) InitializeRow(row);
            else InitializeRow(anchor);

            AddToClassList(USSClasses.TableRow);
        }

        public void RefreshColumnWidths()
        {
            foreach (var columnEntry in TableControl.ColumnData)
            {
                if (!TableControl.ColumnHeaders.TryGetValue(columnEntry.Key, out var header)) continue;

                int columnPosition = TableControl.GetColumnPosition(columnEntry.Key);
                this[columnPosition - 1].style.width = header.style.width;
            }
        }
        
        public void Refresh()
        {
            foreach (var cell in _cells)
            {
                cell.Value.Refresh();
            }
        }

        private void InitializeRow(Row row)
        {
            Clear();

            var columnsByPosition = TableControl.ColumnData.ToDictionary(c => c.Value.Position, c => c.Value);
            columnsByPosition = columnsByPosition.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
                        
            foreach (var columnEntry in columnsByPosition)
            {
                if (!row.Cells.TryGetValue(columnEntry.Key, out var cell)) continue;

                var cellField = CreateCellField(cell);
                if(cellField is CellControl cellControl)
                    _cells.Add(columnEntry.Key, cellControl);
                Add(cellField);
            }
        }
        
        private void InitializeRow(CellAnchor column)
        {
            Clear();

            var orderedRows = TableControl.TableData.OrderedRows;
                        
            for (int i = TableControl.PageManager.FirstRowPosition - 1; i < orderedRows.Count && i < TableControl.PageManager.LastRowPosition; i++)
            {
                var row = orderedRows[i];
                if (!row.Cells.TryGetValue(column.Position, out var cell)) continue;
                
                var cellField = CreateCellField(cell);
                if(cellField is CellControl cellControl)
                    _cells.Add(row.Id, cellControl);
                Add(cellField);
            }
        }

        private VisualElement CreateCellField(Cell cell)
        {
            if(cell == null) return new Label {text = ""};
            var cellControl = CellControlFactory.Create(cell, TableControl);
            return cellControl;
        }

     
    }
}