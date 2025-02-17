using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(GradientCell), CellSizeCalculationMethod.FixedBigCell)]
    internal class GradientCellControl : CellControl
    {
        public GradientCellControl(GradientCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new GradientField()
            {
                value = (Gradient)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}