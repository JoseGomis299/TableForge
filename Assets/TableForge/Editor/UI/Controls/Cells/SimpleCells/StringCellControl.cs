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
            field.RegisterValueChangedCallback(evt => Cell.SetValue(evt.newValue));
            Add(field);
            
            field.AddToClassList("table__cell__content");
            InitializeSize();
            
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float padding = 20f;
            var preferredWidth = EditorStyles.label.CalcSize(new GUIContent(Cell?.GetValue()?.ToString())).x;
            SetDesiredSize(preferredWidth + padding, UiContants.CellHeight);
        }
    }
}