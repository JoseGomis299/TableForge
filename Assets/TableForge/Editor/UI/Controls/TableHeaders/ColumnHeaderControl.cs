using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnHeaderControl : HeaderControl
    {
        public ColumnHeaderControl(CellAnchor cellAnchor, TableControl tableControl) : base(cellAnchor, tableControl)
        {
            AddToClassList(USSClasses.TableHeaderCellHorizontal);

            string title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.ColumnHeaderVisibility);
            var headerLabel = new Label(title);
            headerLabel.AddToClassList(USSClasses.Fill);
            Add(headerLabel);

            TableControl.HorizontalResizer.HandleResize(this);
        }
    }
}