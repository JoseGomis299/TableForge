using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal sealed class TableCornerControl : HeaderControl
    {
        private static readonly TableCornerContextMenuBuilder _contextMenuBuilder = new();

        public ColumnHeaderContainerControl ColumnHeaderContainer { get; }
        public RowHeaderContainerControl RowHeaderContainer { get; }
        public VisualElement RowsContainer { get; }
        
        public TableCornerControl(TableControl tableControl, ColumnHeaderContainerControl columnHeaderContainer, RowHeaderContainerControl rowHeaderContainer, VisualElement rowsContainer)
        {
            AddToClassList(TableVisualizerUss.TableCorner);
            OnEnable(null, tableControl);
            ColumnHeaderContainer = columnHeaderContainer;
            RowHeaderContainer = rowHeaderContainer;
            RowsContainer = rowsContainer;
            
            bool excludeFromManualResizing = false;
            if (TableControl.Parent != null)
            {
                var parentAttributes = CellStaticData.GetSubTableCellAttributes(TableControl.Parent.GetType());
                if (parentAttributes.rowHeaderVisibility == TableHeaderVisibility.Hidden)
                {
                    excludeFromManualResizing = true;
                }
            }
            
            TableControl.HorizontalResizer.HandleResize(this, excludeFromManualResizing);
        }

        protected override IHeaderContextMenuBuilder GetContextMenuBuilder() => _contextMenuBuilder;
    }
}