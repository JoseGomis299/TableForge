using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(ColorCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class ColorCellControl : SimpleCellControl
    {
        private readonly ColorField _field;
        public ColorCellControl(ColorCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new ColorField()
            {
                value = (Color)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt => OnChange(evt, _field));
            Add(_field);
            Field = _field;

            _field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            _field.value = (Color)Cell.GetValue();
        }
    }
}