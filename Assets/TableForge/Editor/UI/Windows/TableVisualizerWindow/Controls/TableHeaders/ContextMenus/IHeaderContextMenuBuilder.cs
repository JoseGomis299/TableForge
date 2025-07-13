using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Interface for building context menus for header controls.
    /// </summary>
    internal interface IHeaderContextMenuBuilder
    {
        /// <summary>
        /// Builds the context menu for a header control.
        /// </summary>
        /// <param name="header">The header control for which to build the menu.</param>
        /// <param name="evt">The contextual menu populate event.</param>
        void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt);
    }
} 