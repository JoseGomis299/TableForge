using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableCornerControl : HeaderControl
    {
        public ColumnHeaderContainerControl ColumnHeaderContainer { get; }
        public RowHeaderContainerControl RowHeaderContainer { get; }
        public VisualElement RowsContainer { get; }
        private TableControl _tableControl;
        private Label _label;
        public TableCornerControl(TableControl tableControl, ColumnHeaderContainerControl columnHeaderContainer, RowHeaderContainerControl rowHeaderContainer, VisualElement rowsContainer) : base(null, tableControl)
        {
            AddToClassList(USSClasses.TableCorner);
            ColumnHeaderContainer = columnHeaderContainer;
            RowHeaderContainer = rowHeaderContainer;
            RowsContainer = rowsContainer;
            _tableControl = tableControl;
            
            _label = new Label();
            _label.AddToClassList(USSClasses.CornerText);
            Add(_label);
            
            TableControl.HorizontalResizer.HandleResize(this);
            this.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));
        }

        // Callback to build the context menu
        void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Next page", NextPage, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Prev page", PrevPage, DropdownMenuAction.AlwaysEnabled);
        }

        void NextPage(DropdownMenuAction action)
        {
            _tableControl.PageManager.NextPage();
        }

        void PrevPage(DropdownMenuAction action)
        {
            _tableControl.PageManager.PreviousPage();
        }

        public void UpdateCornerText()
        {
            _label.text = $"{_tableControl.PageManager.Page} / {_tableControl.PageManager.PageNumber}";
        }
    }
}