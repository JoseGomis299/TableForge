using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnHeaderContainerControl : HeaderContainerControl
    {
        public ColumnHeaderContainerControl(ScrollView cellContainer) : base(cellContainer)
        {
            AddToClassList("table__header-container--horizontal");
            cellContainer.verticalScroller.valueChanged += HandleOffset;
            style.left = UiContants.CellWidth;

            HandleOffset(0);
        }

        private void HandleOffset(float offset)
        {
            style.top = offset;
        }
    }
}