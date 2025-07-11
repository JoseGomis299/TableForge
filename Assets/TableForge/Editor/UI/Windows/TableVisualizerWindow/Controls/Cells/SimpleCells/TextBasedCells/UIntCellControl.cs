using TableForge.Editor.UI.UssClasses;
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

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
    }
}