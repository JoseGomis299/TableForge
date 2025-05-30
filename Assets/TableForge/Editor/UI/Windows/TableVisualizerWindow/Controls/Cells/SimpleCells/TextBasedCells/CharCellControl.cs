using UnityEngine.UIElements;

namespace TableForge.UI
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
            OnRefresh = () => field.value = Cell.GetValue().ToString();
            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
            field.AddToClassList(USSClasses.MultilineCell);
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