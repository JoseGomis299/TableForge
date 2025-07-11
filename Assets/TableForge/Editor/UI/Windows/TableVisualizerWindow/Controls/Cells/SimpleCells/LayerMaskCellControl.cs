using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(LayerMaskCell), CellSizeCalculationMethod.EnumAutoSize)]
    internal class LayerMaskCellControl : SimpleCellControl
    {
        private readonly LayerMaskField _field;
        public LayerMaskCellControl(LayerMaskCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new LayerMaskField()
            {
                value = (LayerMask)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt => OnChange(evt, _field));
            Add(_field);
            Field = _field;

            _field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void SetCellValue(object value)
        {
            if(value is int intValue)
            {
                base.SetCellValue((LayerMask)intValue);
            }
        }

        protected override void OnRefresh()
        {
            _field.value = (LayerMask)Cell.GetValue();
        }
    }
}