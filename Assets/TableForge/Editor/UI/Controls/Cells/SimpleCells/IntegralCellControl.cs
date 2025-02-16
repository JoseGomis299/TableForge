using TableForge.Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class IntegralCellControl : CellControl
    {
        public IntegralCellControl(IntegralCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = GetField();
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            InitializeSize();
            
            IsSelected = false;
        }
        
        private VisualElement GetField()
        {
            if (Cell.Type == typeof(int))
            {
                var field = new IntegerField
                {
                    value = (int)Cell.GetValue()
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

            if (Cell.Type == typeof(long))
            {
                var field = new LongField()
                {
                    value = (long)Cell.GetValue()
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
            
            if (Cell.Type == typeof(uint))
            {
                var field = new UnsignedIntegerField()
                {
                    value = (uint)Cell.GetValue()
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
            
            if (Cell.Type == typeof(ulong))
            {
                var field = new UnsignedLongField()
                {
                    value = (ulong)Cell.GetValue()
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

        protected override void InitializeSize()
        {
            float padding = 20f;
            var preferredWidth = EditorStyles.label.CalcSize(new GUIContent(Cell.GetValue().ToString())).x;
            SetDesiredSize(preferredWidth + padding, UiConstants.CellHeight);
        }
    }
}