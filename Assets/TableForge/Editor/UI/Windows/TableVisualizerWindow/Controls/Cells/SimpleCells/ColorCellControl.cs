using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ColorCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class ColorCellControl : SimpleCellControl
    {
        public ColorCellControl(ColorCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new ColorField()
            {
                value = (Color)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (Color)Cell.GetValue();
            Add(field);
            Field = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}