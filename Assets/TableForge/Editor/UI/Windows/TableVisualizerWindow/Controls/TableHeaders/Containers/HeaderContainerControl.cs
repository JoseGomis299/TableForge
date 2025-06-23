using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class HeaderContainerControl : VisualElement
    {
        protected TableControl tableControl;
        
        protected HeaderContainerControl(TableControl tableControl)
        {
            this.tableControl = tableControl;
        }
        
    }
}