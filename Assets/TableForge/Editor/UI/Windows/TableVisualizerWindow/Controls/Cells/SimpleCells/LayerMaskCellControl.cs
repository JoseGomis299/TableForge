using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(LayerMaskCell), CellSizeCalculationMethod.EnumAutoSize)]
    internal class LayerMaskCellControl : SimpleCellControl
    {
        public LayerMaskCellControl(LayerMaskCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new LayerMaskField()
            {
                value = (LayerMask)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (LayerMask)Cell.GetValue();
            Add(field);
            Field = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }

        protected override void SetCellValue(object value)
        {
            if(value is int intValue)
            {
                base.SetCellValue((LayerMask)intValue);
            }
        }
    }
}