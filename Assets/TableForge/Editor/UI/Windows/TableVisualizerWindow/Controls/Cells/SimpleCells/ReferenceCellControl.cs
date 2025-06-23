using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(ReferenceCell), CellSizeCalculationMethod.ReferenceAutoSize)]
    internal class ReferenceCellControl : SimpleCellControl
    {
        public ReferenceCellControl(ReferenceCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new ObjectField()
            {
                value = (Object)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (Object)Cell.GetValue();
            Add(field);
            Field = field;

            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}