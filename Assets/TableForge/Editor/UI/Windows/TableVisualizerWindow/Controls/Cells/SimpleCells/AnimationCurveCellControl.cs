using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;
using CurveField = UnityEditor.UIElements.CurveField;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(AnimationCurveCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class AnimationCurveCellControl : SimpleCellControl
    {
        private readonly CurveField _field;
        public AnimationCurveCellControl(AnimationCurveCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _field = new CurveField()
            {
                value = (AnimationCurve)Cell.GetValue()
            };
            _field.RegisterValueChangedCallback(evt =>
            {
                //We need to create a new AnimationCurve to avoid the reference being shared between cells when re-utilizing this cellControl
                var cachedEvt = ChangeEvent<AnimationCurve>.GetPooled(evt.previousValue, new AnimationCurve(evt.newValue.keys));
                OnChange(cachedEvt, _field);
            });
            
            Add(_field);
            
            Field = _field;
            _field.AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            _field.value = (AnimationCurve)Cell.GetValue();
        }
    }
}