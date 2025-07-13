using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Context menu builder for table corner that handles corner-specific menu items.
    /// </summary>
    internal class TableCornerContextMenuBuilder : BaseHeaderContextMenuBuilder
    {
        public override void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            if (header is not TableCornerControl) return;
            
            AddExpandCollapseItems(header, evt);
            evt.menu.AppendSeparator();
        }
    }
}