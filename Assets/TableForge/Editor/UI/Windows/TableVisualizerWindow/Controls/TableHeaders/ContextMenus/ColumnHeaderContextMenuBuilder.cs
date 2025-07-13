using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Context menu builder for column headers that handles column-specific menu items.
    /// </summary>
    internal class ColumnHeaderContextMenuBuilder : BaseHeaderContextMenuBuilder
    {
        public override void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            if (header is not ColumnHeaderControl)
                return;
            
            AddExpandCollapseItems(header, evt);
            evt.menu.AppendSeparator();
            AddSortingItems(header, evt);
        }
    }
} 