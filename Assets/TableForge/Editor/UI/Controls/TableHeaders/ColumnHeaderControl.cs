using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnHeaderControl : HeaderControl
    {
        public ColumnHeaderControl(int id, string name, TableControl tableControl) : base(id, name, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellHorizontal);
            
            var textSize = EditorStyles.label.CalcSize(new GUIContent(CompleteName.Replace("<b>", "").Replace("</b>", "")));
            var desiredWidth = textSize.x + UiConstants.HeaderPadding;
            TableControl.ColumnData[Id].AddPreferredWidth(id, Mathf.Max(desiredWidth, UiConstants.MinCellWidth));

            var headerLabel = new Label(CompleteName);
            headerLabel.AddToClassList(USSClasses.Fill);
            Add(headerLabel);

            TableControl.HorizontalResizer.HandleResize(this);
        }
        
        private string CompleteName => $"{TableControl.ColumnData[Id].CellAnchor.LetterPosition} | <b>{Name}</b>";
    }
}