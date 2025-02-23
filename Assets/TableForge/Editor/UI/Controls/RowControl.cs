using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class RowControl : VisualElement
    {
        private readonly Dictionary<int, CellControl> _cells = new Dictionary<int, CellControl>();
        public IReadOnlyDictionary<int, CellControl> Cells => _cells;
        public Row Row { get; }
        public TableControl TableControl { get; }
        
        public RowControl(Row row, TableControl tableControl)
        {
            Row = row;
            TableControl = tableControl;
            InitializeRow();

            AddToClassList(USSClasses.TableRow);
            AddToClassList(USSClasses.Hidden);
        }

        public void RefreshColumnWidths()
        {
            foreach (var columnEntry in TableControl.ColumnData)
            {
                if (!TableControl.ColumnHeaders.TryGetValue(columnEntry.Key, out var header)) continue;
                if(childCount <= columnEntry.Value.Position - 1) continue;
                this[columnEntry.Value.Position - 1].style.width = header.style.width;
            }
        }
        
        public void Refresh()
        {
            foreach (var cell in _cells)
            {
                cell.Value.Refresh();
            }
        }

        private void InitializeRow()
        {
            Clear();

            var columnsByPosition = TableControl.ColumnData.ToDictionary(c => c.Value.Position, c => c.Value);
            columnsByPosition = columnsByPosition.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
                        
            foreach (var columnEntry in columnsByPosition)
            {
                if (!Row.Cells.TryGetValue(columnEntry.Key, out var cell)) continue;

                var cellField = CreateCellField(cell, columnEntry.Value.PreferredWidth);
                if(cellField is CellControl cellControl)
                    _cells.Add(columnEntry.Key, cellControl);
                Add(cellField);
            }
        }

        private VisualElement CreateCellField(Cell cell, float columnWidth)
        {
            if(cell == null) return new Label {text = ""};
            var cellControl = CellControlFactory.Create(cell, TableControl);
            cellControl.style.width = columnWidth;
            return cellControl;
        }

     
    }
}