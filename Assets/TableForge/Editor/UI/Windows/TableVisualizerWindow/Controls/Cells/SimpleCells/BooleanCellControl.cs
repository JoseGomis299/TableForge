using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(BoolCell), CellSizeCalculationMethod.FixedSmallCell)]
    internal class BooleanCellControl : SimpleCellControl
    {
        public BooleanCellControl(BoolCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new Toggle
            {
                value = (bool)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            OnRefresh = () => field.value = (bool)Cell.GetValue();
            Add(field);
            Field = field;
            
            field.AddToClassList(USSClasses.Fill);
            field.AddToChildrenClassList(USSClasses.Center); 
        }
    }
}