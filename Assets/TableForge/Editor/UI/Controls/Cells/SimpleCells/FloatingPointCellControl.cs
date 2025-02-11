using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class FloatingPointCellControl : CellControl
    {
        public FloatingPointCellControl(FloatingPointCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = GetField();
            Add(field);
            
            field.AddToClassList("table__cell__content");
            InitializeSize();
            
            IsSelected = false;
        }
        
        private VisualElement GetField()
        {
            if (Cell.Type == typeof(float))
            {
                var field = new FloatField
                {
                    value = (float)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(OnChange);
                return field;
            }

            if (Cell.Type == typeof(double))
            {
                var field = new DoubleField
                {
                    value = (double)Cell.GetValue()
                };
                field.RegisterValueChangedCallback(OnChange);
                return field;
            }

            return null;
        }


        protected override void InitializeSize()
        {
            float padding = 20f;
            var preferredWidth = EditorStyles.label.CalcSize(new GUIContent(Cell.GetValue().ToString())).x;
            SetDesiredSize(preferredWidth + padding, UiContants.CellHeight);
        }
    }
}