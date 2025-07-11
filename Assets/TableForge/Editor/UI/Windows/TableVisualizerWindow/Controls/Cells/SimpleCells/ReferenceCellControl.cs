using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(ReferenceCell), CellSizeCalculationMethod.ReferenceAutoSize)]
    internal class ReferenceCellControl : SimpleCellControl
    {
        private readonly ObjectField _field;
        public ReferenceCellControl(ReferenceCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new ObjectField()
            {
                value = (Object)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt => OnChange(evt, _field));
            Add(_field);
            Field = _field;

            _field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            _field.value = (Object)Cell.GetValue();
        }
    }
}