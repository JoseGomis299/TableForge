using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class RowControl : VisualElement
    {
        private readonly Row _row;
        private readonly Dictionary<int, CellControl> _cells = new Dictionary<int, CellControl>();
        
        public IReadOnlyDictionary<int, CellControl> Cells => _cells;
        public TableControl TableControl { get; }
        
        public RowControl(Row row, TableControl tableControl)
        {
            _row = row;
            TableControl = tableControl;
            SetRow();

            AddToClassList(USSClasses.TableRow);
            AddToClassList(USSClasses.Hidden);
        }

        public void RefreshColumnWidths()
        {
            foreach (var columnEntry in TableControl.ColumnData)
            {
                if (!TableControl.ColumnHeaders.TryGetValue(columnEntry.Key, out var header)) continue;
                this[columnEntry.Value.Position - 1].style.width = header.style.width;
            }
        }

        private void SetRow()
        {
            Clear();

            var columnsByPosition = TableControl.ColumnData.ToDictionary(c => c.Value.Position, c => c.Value);
            columnsByPosition = columnsByPosition.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
                        
            foreach (var columnEntry in columnsByPosition)
            {
                if (!_row.Cells.TryGetValue(columnEntry.Key, out var cell)) continue;

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
                    return new Label { text = "" };
            }
        }
    }
}