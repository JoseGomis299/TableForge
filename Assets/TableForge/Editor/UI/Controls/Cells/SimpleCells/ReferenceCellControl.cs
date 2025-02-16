using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ReferenceCell), CellSizeCalculationMethod.AutoSize)]
    internal class ReferenceCellControl : CellControl
    {
        public ReferenceCellControl(ReferenceCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            var field = new ObjectField()
            {
                value = (Object)Cell.GetValue()
            };
            field.RegisterValueChangedCallback(OnChange);
            Add(field);
            
            field.AddToClassList(USSClasses.TableCellContent);
            
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float padding = 50f; 
            var preferredWidth = Cell.GetValue() as Object != null ? 
                EditorStyles.label.CalcSize(new GUIContent((Cell.GetValue() as Object)?.name)).x :
                EditorStyles.label.CalcSize(new GUIContent($"None ({Cell.Type.Name})")).x;
                
            SetDesiredSize(preferredWidth + padding, UiConstants.CellHeight);
        }
    }
}