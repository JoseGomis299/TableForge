using UnityEngine;
using UnityEngine.UIElements;
using CurveField = UnityEditor.UIElements.CurveField;

namespace TableForge.UI
{
    [CellControlUsage(typeof(AnimationCurveCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class AnimationCurveCellControl : SimpleCellControl
    {
        public AnimationCurveCellControl(AnimationCurveCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new CurveField()
            {
                value = (AnimationCurve)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt =>
            {
                //We need to create a new AnimationCurve to avoid the reference being shared between cells when re-utilizing this cellControl
                var cachedEvt = ChangeEvent<AnimationCurve>.GetPooled(evt.previousValue, new AnimationCurve(evt.newValue.keys));
                OnChange(cachedEvt, field);
            });
            
            OnRefresh = () =>
            {
                field.value = (AnimationCurve)Cell.GetValue();
            };
            Add(field);
            
            Field = field;
            field.AddToClassList(USSClasses.TableCellContent);
        }
    }
}