using TableForge.Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class StringCellControl : CellControl
    {
        public StringCellControl(StringCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new TextField
            {
                value = (string)Cell.GetValue()
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
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            InitializeSize();
            
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float padding = 20f;
            var preferredWidth = EditorStyles.label.CalcSize(new GUIContent(Cell.GetValue()?.ToString())).x;
            SetDesiredSize(preferredWidth + padding, UiConstants.CellHeight);
        }
    }
}