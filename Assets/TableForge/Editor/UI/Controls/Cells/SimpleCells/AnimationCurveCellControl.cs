using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
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
            
            field.AddToClassList("table__cell__content");
            SetDesiredSize(UiContants.SpecialCellDesiredWidth, UiContants.CellHeight);
            
            IsSelected = false;
        }
    }
}