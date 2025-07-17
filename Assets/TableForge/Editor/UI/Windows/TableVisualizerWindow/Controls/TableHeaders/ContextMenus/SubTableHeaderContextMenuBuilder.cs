using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Context menu builder for sub-table headers that handles sub-table specific menu items.
    /// </summary>
    internal class SubTableHeaderContextMenuBuilder : BaseHeaderContextMenuBuilder
    {
        public override void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt)
        {
            AddExpandCollapseItems(header, evt);
        }
    }
}