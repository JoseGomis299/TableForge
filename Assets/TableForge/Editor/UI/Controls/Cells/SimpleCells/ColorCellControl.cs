using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ColorCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class ColorCellControl : CellControl
    {
        public ColorCellControl(ColorCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new ColorField()
            {
                value = (Color)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}