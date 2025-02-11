using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnHeaderControl : HeaderControl
    {
        public ColumnHeaderControl(int id, string name, TableControl tableControl) : base(id, name, tableControl)
        {
            AddToClassList("table__header-cell");
            
            var textSize = EditorStyles.label.CalcSize(new GUIContent(CompleteName.Replace("<b>", "").Replace("</b>", "")));
            var desiredWidth = textSize.x + UiContants.HeaderPadding;
            TableControl.ColumnData[Id].AddPreferredWidth(id, Mathf.Max(desiredWidth, UiContants.MinCellWidth));

            var headerLabel = new Label(CompleteName);
            headerLabel.AddToClassList("fill");
            Add(headerLabel);

            TableControl.HorizontalResizer.HandleResize(this);
        }
        
        private string CompleteName => Id == 0 ? "" : $"{TableControl.ColumnData[Id].CellAnchor.LetterPosition} | <b>{Name}</b>";
    }
}