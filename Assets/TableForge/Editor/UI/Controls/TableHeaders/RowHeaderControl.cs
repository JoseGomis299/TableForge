using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        public RowControl RowControl { get; }

        public RowHeaderControl(int id, string name, TableControl tableControl, RowControl rowControl) : base(id, name, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);
            AddToClassList(USSClasses.Hidden);
            RowControl = rowControl;
            
            var textSize = EditorStyles.label.CalcSize(new GUIContent(CompleteName.Replace("<b>", "").Replace("</b>", "")));
            var desiredWidth = textSize.x + UiConstants.HeaderPadding;
            TableControl.ColumnData[0].AddPreferredWidth(id, Mathf.Max(desiredWidth, TableControl.ColumnData[0].PreferredWidth));

            var headerLabel = new Label(CompleteName);
            headerLabel.AddToClassList(USSClasses.Fill);
            Add(headerLabel);

            TableControl.VerticalResizer.HandleResize(this);
        }

        private string CompleteName => $"{TableControl.RowData[Id].CellAnchor.Position} | <b>{Name}</b>";
    }
}