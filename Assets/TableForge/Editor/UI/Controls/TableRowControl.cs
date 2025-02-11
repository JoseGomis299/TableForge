using System.Linq;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class TableRowControl : VisualElement
    {
        private readonly Row _row;
        private readonly TableControl _tableControl;

        public TableRowControl(Row row, TableControl tableControl)
        {
            _row = row;
            _tableControl = tableControl;
            SetRow();

            AddToClassList("table__row");
        }

        public void SetColumnWidth(int id, float width)
        {
            this[_tableControl.ColumnData[id].Position].style.width = width;
        }

        private void SetRow()
        {
            Clear();

            var columnsByPosition = _tableControl.ColumnData.ToDictionary(c => c.Value.Position, c => c.Value);
            columnsByPosition = columnsByPosition.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

            var header = new RowHeaderControl(_row.Id, _row.Name, _tableControl);
            Add(header);

            foreach (var columnEntry in columnsByPosition)
            {
                if (!_row.Cells.TryGetValue(columnEntry.Key, out var cell)) continue;

                var cellField = CreateCellField(cell, columnEntry.Value.PreferredWidth);
                Add(cellField);
            }
        }

        private VisualElement CreateCellField(Cell cell, float columnWidth)
        {
            if(cell == null) return new Label {text = ""};
            var cellControl = CellControlFactory.Create(cell, _tableControl);
            cellControl.style.width = columnWidth;
            return cellControl;
        }
    }
    
    internal static class CellControlFactory
    {
        public static VisualElement Create(Cell cell, TableControl tableControl)
        {
            switch (cell)
            {
                case BoolCell boolCell:
                    return new BooleanCellControl(boolCell, tableControl);
                case IntegralCell integralCell:
                    return new IntegralCellControl(integralCell, tableControl);
                case FloatingPointCell floatCell:
                    return new FloatingPointCellControl(floatCell, tableControl);
                case StringCell stringCell:
                    return new StringCellControl(stringCell, tableControl);
                case EnumCell enumCell:
                    return new EnumCellControl(enumCell, tableControl);
                case AnimationCurveCell animationCurveCell:
                    return new AnimationCurveCellControl(animationCurveCell, tableControl);
                case ColorCell colorCell:
                    return new ColorCellControl(colorCell, tableControl);
                case GradientCell gradientCell:
                    return new GradientCellControl(gradientCell, tableControl);
                case ReferenceCell referenceCell:
                    return new ReferenceCellControl(referenceCell, tableControl);
                default:
                    return new Label {text = ""};
            }
        }
    }
}