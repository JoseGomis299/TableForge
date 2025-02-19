using TableForge.Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(StringCell), CellSizeCalculationMethod.AutoSize)]
    internal class StringCellControl : CellControl
    {
        public StringCellControl(StringCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new TextField
            {
                value = (string)Cell.GetValue(),
            };
            field.RegisterValueChangedCallback(evt => OnChange(evt, field));
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            field.AddToClassList(USSClasses.MultilineCell);
            IsSelected = false;
            
            InitializeSize();
        }
    }
}