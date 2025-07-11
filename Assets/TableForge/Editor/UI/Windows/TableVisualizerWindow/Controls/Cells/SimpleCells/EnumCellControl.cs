using System;
using System.Reflection;
using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(EnumCell), CellSizeCalculationMethod.EnumAutoSize)]
    internal class EnumCellControl : SimpleCellControl
    {
        private readonly EnumFlagsField _flagsField;
        private readonly EnumField _enumField;
        
        public EnumCellControl(EnumCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            if(cell.Type.GetCustomAttribute<FlagsAttribute>() != null)
            {
                _flagsField = new EnumFlagsField(Cell.GetValue() as Enum);
                _flagsField.RegisterValueChangedCallback(evt => OnChange(evt, _flagsField));
                Add(_flagsField);
                Field = _flagsField;
            }
            else
            {
                _enumField = new EnumField(Cell.GetValue() as Enum);
                _enumField.RegisterValueChangedCallback(evt => OnChange(evt, _enumField));
                Add(_enumField);
                Field = _enumField;
            }

            this[0].AddToClassList(TableVisualizerUss.TableCellContent);
        }

        protected override void OnRefresh()
        {
            if(_enumField != null)
            {
                _enumField.value = Cell.GetValue() as Enum;
            }
            else if(_flagsField != null)
            {
                _flagsField.value = Cell.GetValue() as Enum;
            }
        }
    }
}