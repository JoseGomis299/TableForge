using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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