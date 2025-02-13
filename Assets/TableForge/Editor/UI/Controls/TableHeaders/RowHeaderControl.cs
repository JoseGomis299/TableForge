using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        public TableRowControl RowControl { get; }

        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
              
            }
        }

        public RowHeaderControl(int id, string name, TableControl tableControl, TableRowControl rowControl) : base(id, name, tableControl)
        {
            AddToClassList("table__header-cell--vertical");
            RowControl = rowControl;
            
            var textSize = EditorStyles.label.CalcSize(new GUIContent(CompleteName.Replace("<b>", "").Replace("</b>", "")));
            var desiredWidth = textSize.x + UiContants.HeaderPadding;
            TableControl.ColumnData[0].AddPreferredWidth(id, Mathf.Max(desiredWidth, TableControl.ColumnData[0].PreferredWidth));

            var headerLabel = new Label(CompleteName);
            headerLabel.AddToClassList("fill");
            Add(headerLabel);

            TableControl.VerticalResizer.HandleResize(this);
        }
        
        
        private string CompleteName => $"{TableControl.RowData[Id].CellAnchor.Position} | <b>{Name}</b>";
    }
}