using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(UIntCell), CellSizeCalculationMethod.AutoSize)]
    internal class UIntCellControl : TextBasedCellControl<uint>
    {
        public UIntCellControl(UIntCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new UnsignedIntegerField
            {
                value = (uint)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (uint)Cell.GetValue();

            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}