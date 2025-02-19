using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(EnumCell), CellSizeCalculationMethod.EnumAutoSize)]
    internal class EnumCellControl : CellControl
    {
        public EnumCellControl(EnumCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            if(cell.Type.GetCustomAttribute<FlagsAttribute>() != null)
            {
                var field = new EnumFlagsField(Cell.GetValue() as Enum);
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                Add(field);
            }
            else
            {
                var field = new EnumField(Cell.GetValue() as Enum);
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                Add(field);
            }
            
            this[0].AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}