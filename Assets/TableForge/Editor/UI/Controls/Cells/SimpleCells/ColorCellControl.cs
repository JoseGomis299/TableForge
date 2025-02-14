using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColorCellControl : CellControl
    {
        public ColorCellControl(ColorCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new ColorField()
            {
                value = (Color)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            SetDesiredSize(UiConstants.SpecialCellDesiredWidth, UiConstants.CellHeight);
            
            IsSelected = false;
        }
    }
}