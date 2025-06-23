using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(EnumCell), CellSizeCalculationMethod.EnumAutoSize)]
    internal class EnumCellControl : SimpleCellControl
    {
        public EnumCellControl(EnumCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            if(cell.Type.GetCustomAttribute<FlagsAttribute>() != null)
            {
                var field = new EnumFlagsField(Cell.GetValue() as Enum);
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh += () => field.value = Cell.GetValue() as Enum;
                Add(field);
                Field = field;
            }
            else
            {
                var field = new EnumField(Cell.GetValue() as Enum);
                field.RegisterValueChangedCallback(evt => OnChange(evt, field));
                OnRefresh += () => field.value = Cell.GetValue() as Enum;
                Add(field);
                Field = field;
            }

            this[0].AddToClassList(USSClasses.TableCellContent);
        }
    }
}