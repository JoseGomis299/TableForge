using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableCornerControl : HeaderControl
    {
        public ColumnHeaderContainerControl ColumnHeaderContainer { get; }
        public RowHeaderContainerControl RowHeaderContainer { get; }
        public VisualElement RowsContainer { get; }
        private TableControl _tableControl;
        public TableCornerControl(TableControl tableControl, ColumnHeaderContainerControl columnHeaderContainer, RowHeaderContainerControl rowHeaderContainer, VisualElement rowsContainer) : base(null, tableControl)
        {
            AddToClassList(USSClasses.TableCorner);
            ColumnHeaderContainer = columnHeaderContainer;
            RowHeaderContainer = rowHeaderContainer;
            RowsContainer = rowsContainer;
            _tableControl = tableControl;
            
            TableControl.HorizontalResizer.HandleResize(this);
            this.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));
        }

        // Callback to build the context menu
        void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            if(_tableControl.Parent == null)
                evt.menu.AppendAction("Transpose table", TransposeTable, DropdownMenuAction.AlwaysEnabled);
        }
        
        
        void TransposeTable(DropdownMenuAction action)
        {
            _tableControl.CellSelector.ClearSelection();

            _tableControl.Transpose();
            _tableControl.RebuildPage();
        }
    }
}