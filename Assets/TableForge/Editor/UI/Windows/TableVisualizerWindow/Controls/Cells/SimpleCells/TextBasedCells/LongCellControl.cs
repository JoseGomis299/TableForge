using TableForge.Editor.UI.UssClasses;
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

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
    }
}