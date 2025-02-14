using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class EnumCellControl : CellControl
    {
        public EnumCellControl(EnumCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new EnumField(Cell.GetValue() as Enum);
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            InitializeSize();
            
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float padding = 60f;
            var preferredWidth = EditorStyles.label.CalcSize(new GUIContent((Cell.GetValue() as Enum)?.ToString())).x;
                
            SetDesiredSize(preferredWidth + padding, UiConstants.CellHeight);
        }
    }
}