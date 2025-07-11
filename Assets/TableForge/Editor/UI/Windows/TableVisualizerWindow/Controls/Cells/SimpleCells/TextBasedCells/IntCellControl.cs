using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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

            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
    }
}