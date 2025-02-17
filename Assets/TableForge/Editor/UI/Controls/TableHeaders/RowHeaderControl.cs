using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        public RowControl RowControl { get; }

        public RowHeaderControl(CellAnchor cellAnchor, TableControl tableControl, RowControl rowControl) : base(cellAnchor, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);
            AddToClassList(USSClasses.Hidden);
            RowControl = rowControl;


            float preferredWidth = SizeCalculator.CalculateHeaderSize(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility).x;
            TableControl.ColumnData[0].AddPreferredWidth(Id, Mathf.Max(preferredWidth, TableControl.ColumnData[0].PreferredWidth));

            string title = HeaderNameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            var headerLabel = new Label(title);
            headerLabel.AddToClassList(USSClasses.Fill);
            Add(headerLabel);

            TableControl.VerticalResizer.HandleResize(this);
        }
    }
}