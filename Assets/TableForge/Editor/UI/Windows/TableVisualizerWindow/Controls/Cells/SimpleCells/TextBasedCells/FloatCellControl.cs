using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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
            
            TextField = field;
            Add(field);
            
            field.AddToClassList(TableVisualizerUss.TableCellContent);
        }
    }
}