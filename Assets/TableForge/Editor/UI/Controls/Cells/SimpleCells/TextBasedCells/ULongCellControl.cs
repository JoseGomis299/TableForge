using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ULongCell), CellSizeCalculationMethod.AutoSize)]
    internal class ULongCellControl : TextBasedCellControl<ulong>
    {
        public ULongCellControl(ULongCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new UnsignedLongField
            {
                value = (ulong)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (ulong)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}