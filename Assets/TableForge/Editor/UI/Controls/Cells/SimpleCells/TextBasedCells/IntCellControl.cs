using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(IntCell), CellSizeCalculationMethod.AutoSize)]
    internal class IntCellControl : TextBasedCellControl<int>
    {
        public IntCellControl(IntCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new IntegerField
            {
                value = (int)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (int)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}