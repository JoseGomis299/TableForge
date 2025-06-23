using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(LongCell), CellSizeCalculationMethod.AutoSize)]
    internal class LongCellControl : TextBasedCellControl<long>
    {
        public LongCellControl(LongCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new LongField
            {
                value = (long)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (long)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}