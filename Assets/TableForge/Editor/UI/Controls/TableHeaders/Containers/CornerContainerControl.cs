using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CornerContainerControl : HeaderContainerControl
    {
        public CornerContainerControl(ScrollView cellContainer) : base(cellContainer)
        {
            AddToClassList("table__corner-container");
            cellContainer.horizontalScroller.valueChanged += HandleOffset;
            cellContainer.verticalScroller.valueChanged += HandleVerticalOffset;
        }

        private void HandleOffset(float offset)
        {
            style.left = offset;
        }
        
        private void HandleVerticalOffset(float offset)
        {
            style.top = offset;
        }
    }
}