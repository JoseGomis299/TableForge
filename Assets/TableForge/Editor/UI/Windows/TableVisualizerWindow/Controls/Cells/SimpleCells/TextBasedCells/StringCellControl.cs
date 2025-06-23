using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(StringCell), CellSizeCalculationMethod.AutoSize)]
    internal class StringCellControl : TextBasedCellControl<string>
    {
        public StringCellControl(StringCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new TextField
            {
                value = (string)Cell.GetValue(),
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            Add(field);
            TextField = field;

            field.AddToClassList(USSClasses.TableCellContent);
            field.AddToClassList(USSClasses.MultilineCell);
        }
    }
}