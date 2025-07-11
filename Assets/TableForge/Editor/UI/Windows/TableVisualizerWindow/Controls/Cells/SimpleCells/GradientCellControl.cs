using System.Linq;
using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(GradientCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class GradientCellControl : SimpleCellControl
    {
        private readonly GradientField _field;
        public GradientCellControl(GradientCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new GradientField()
            {
                value = (Gradient)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt =>
            {
                //We need to create a new Gradient to avoid the reference being shared between cells when re-utilizing this cellControl
                Gradient newGradient = new Gradient();
                newGradient.alphaKeys = evt.newValue.alphaKeys.ToArray();
                newGradient.colorKeys = evt.newValue.colorKeys.ToArray();
                
                var cachedEvt = ChangeEvent<Gradient>.GetPooled(evt.previousValue, newGradient);
                OnChange(cachedEvt, _field);
            });            
            Add(_field);
            Field = _field;

            _field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            _field.value = (Gradient)Cell.GetValue();
        }
    }
}