using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderContainerControl : VisualElement
    {
        protected TableControl TableControl;
        
        protected HeaderContainerControl(TableControl tableControl)
        {
            TableControl = tableControl;
        }
        
    }
}