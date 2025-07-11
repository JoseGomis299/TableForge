using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
    }
}