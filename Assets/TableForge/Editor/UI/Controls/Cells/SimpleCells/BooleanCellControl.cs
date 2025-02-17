using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(BoolCell), CellSizeCalculationMethod.FixedSmallCell)]
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
            
            field.AddToClassList(USSClasses.Fill);
            field.AddToChildrenClassList(USSClasses.Center);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}