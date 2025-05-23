using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(FloatCell), CellSizeCalculationMethod.AutoSize)]
    internal class FloatCellControl : TextBasedCellControl<float>
    {
        public FloatCellControl(FloatCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new FloatField
            {
                value = (float)Cell.GetValue()
            };
            
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (float)Cell.GetValue();
            
            TextField = field;
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}