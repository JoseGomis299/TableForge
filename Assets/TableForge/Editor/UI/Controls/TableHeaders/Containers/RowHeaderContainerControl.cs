using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderContainerControl : HeaderContainerControl
    {
        public RowHeaderContainerControl(ScrollView cellContainer) : base(cellContainer)
        {
            AddToClassList("table__header-container--vertical");
            cellContainer.horizontalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.left = offset;
        }
    }
}