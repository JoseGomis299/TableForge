using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class GradientCellControl : CellControl
    {
        public GradientCellControl(GradientCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new GradientField()
            {
                value = (Gradient)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => Cell.SetValue(evt.newValue));
            Add(field);
            
            field.AddToClassList("table__cell__content");
            SetDesiredSize(UiContants.SpecialCellDesiredWidth, UiContants.CellHeight);
            
            IsSelected = false;
        }
    }
}