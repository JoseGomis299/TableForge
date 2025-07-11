using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(BoolCell), CellSizeCalculationMethod.FixedSmallCell)]
    internal class BooleanCellControl : SimpleCellControl
    {
        private readonly Toggle _field;
        public BooleanCellControl(BoolCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new Toggle
            {
                value = (bool)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt => OnChange(evt, _field));
            Add(_field);
            Field = _field;
            
            _field.AddToClassList(TableVisualizerUss.Fill);
            _field.AddToChildrenClassList(TableVisualizerUss.Center); 
        }

        protected override void OnRefresh()
        {
            _field.value = (bool)Cell.GetValue();
        }
    }
}