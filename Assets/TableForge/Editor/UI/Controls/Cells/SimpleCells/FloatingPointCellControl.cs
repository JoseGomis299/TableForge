using TableForge.Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(FloatingPointCell), CellSizeCalculationMethod.AutoSize)]
    internal class FloatingPointCellControl : CellControl
    {
        public FloatingPointCellControl(FloatingPointCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = GetField();
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            IsSelected = false;
            
            InitializeSize();
        }
        
        private VisualElement GetField()
        {
            if (Cell.Type == typeof(float))
            {
                var field = new FloatField
                {
                    value = (float)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt =>
                {
                    try
                    {
                        OnChange(evt);
                    }
                    catch (InvalidCellValueException)
                    {
                        field.SetValueWithoutNotify(evt.previousValue);
                    }
                });
                return field;
            }

            if (Cell.Type == typeof(double))
            {
                var field = new DoubleField
                {
                    value = (double)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(evt =>
                {
                    try
                    {
                        OnChange(evt);
                    }
                    catch (InvalidCellValueException)
                    {
                        field.SetValueWithoutNotify(evt.previousValue);
                    }
                });
                return field;
            }

            return null;
        }
    }
}