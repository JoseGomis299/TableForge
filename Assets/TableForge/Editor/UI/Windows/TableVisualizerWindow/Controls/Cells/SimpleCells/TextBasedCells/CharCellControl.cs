using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(CharCell), CellSizeCalculationMethod.AutoSize)]
    internal class CharCellControl : TextBasedCellControl<string>
    {
        public CharCellControl(CharCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new TextField
            {
                value = Cell.GetValue().ToString(),
                maxLength = 1
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            Add(field);
            TextField = field;

            field.AddToClassList(TableVisualizerUss.TableCellContent);
            field.AddToClassList(TableVisualizerUss.MultilineCell);
        }

        protected override void OnRefresh()
        {
            TextField.value = Cell.GetValue().ToString();
        }

        protected override void SetCellValue(object value)
        {
            if (value is string { Length: > 0 } strValue)
            {
                base.SetCellValue(strValue[0]);
            }
            else
            {
                base.SetCellValue('\0');
            }
        }
    }
}