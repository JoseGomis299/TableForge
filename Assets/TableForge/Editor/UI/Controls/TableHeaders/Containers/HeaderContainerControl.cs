using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderContainerControl : VisualElement
    {
        protected ScrollView CellContainer;
        
        protected HeaderContainerControl(ScrollView cellContainer)
        {
            CellContainer = cellContainer;
        }
        
    }
}