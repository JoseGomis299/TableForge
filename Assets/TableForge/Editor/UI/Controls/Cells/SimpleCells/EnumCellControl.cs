using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(EnumCell), CellSizeCalculationMethod.AutoSize)]
    internal class EnumCellControl : CellControl
    {
        public EnumCellControl(EnumCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new EnumField(Cell.GetValue() as Enum);
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }

        protected override void InitializeSize()
        {
            float padding = 12;
            var preferredWidth = SizeCalculator.CalculateSize(this).x;
                
            SetDesiredSize(preferredWidth + padding, UiConstants.CellHeight);
        }
    }
}