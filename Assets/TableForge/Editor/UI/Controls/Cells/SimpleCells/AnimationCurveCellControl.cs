using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(AnimationCurveCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class AnimationCurveCellControl : CellControl
    {
        public AnimationCurveCellControl(AnimationCurveCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new CurveField()
            {
                value = (AnimationCurve)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            
            IsSelected = false;
        }
    }
}