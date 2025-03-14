using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderControl : HeaderControl
    {
        public RowControl RowControl { get; set; }

        private readonly Label _headerLabel;

        public RowHeaderControl(CellAnchor cellAnchor, TableControl tableControl) : base(cellAnchor, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellVertical);

            float preferredWidth = SizeCalculator.CalculateHeaderSize(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility).x;
            TableControl.ColumnData[0].AddPreferredWidth(Id, Mathf.Max(preferredWidth, TableControl.ColumnData[0].PreferredWidth));

            string title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.RowHeaderVisibility);
            _headerLabel = new Label(title);
            _headerLabel.AddToClassList(USSClasses.Fill);
            Add(_headerLabel);

            TableControl.VerticalResizer.HandleResize(this);
        }
        
        public void Refresh()
        {
            RowControl.Refresh();
            _headerLabel.text = NameResolver.ResolveHeaderStyledName(CellAnchor, TableControl.TableAttributes.RowHeaderVisibility);
        }
    }
}