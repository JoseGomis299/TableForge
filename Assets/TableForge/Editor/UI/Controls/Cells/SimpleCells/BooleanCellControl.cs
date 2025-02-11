using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class BooleanCellControl : CellControl
    {
        public BooleanCellControl(BoolCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new Toggle
            {
                value = (bool)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList("fill");
            field.AddToChildrenClassList("center");
            
            IsSelected = false;
        }
    }
}