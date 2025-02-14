using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableCornerControl : HeaderControl
    {
        public ColumnHeaderContainerControl ColumnHeaderContainer { get; }
        public RowHeaderContainerControl RowHeaderContainer { get; }
        public VisualElement RowsContainer { get; }
        public TableCornerControl(TableControl tableControl, ColumnHeaderContainerControl columnHeaderContainer, RowHeaderContainerControl rowHeaderContainer, VisualElement rowsContainer) : base(0, "", tableControl)
        {
            AddToClassList(USSClasses.TableCorner);
            ColumnHeaderContainer = columnHeaderContainer;
            RowHeaderContainer = rowHeaderContainer;
            RowsContainer = rowsContainer;
            
            TableControl.HorizontalResizer.HandleResize(this);
        }

    }
}