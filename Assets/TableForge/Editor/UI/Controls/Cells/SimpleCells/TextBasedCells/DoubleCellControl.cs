using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(DoubleCell), CellSizeCalculationMethod.AutoSize)]
    internal class DoubleCellControl : TextBasedCellControl<double>
    {
        public DoubleCellControl(DoubleCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new DoubleField
            {
                value = (double)Cell.GetValue()
            };
            
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (double)Cell.GetValue();
            
            TextField = field;
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}