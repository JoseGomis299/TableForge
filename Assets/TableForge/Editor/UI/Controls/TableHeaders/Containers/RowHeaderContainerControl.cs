using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderContainerControl : HeaderContainerControl
    {
        public RowHeaderContainerControl(ScrollView cellContainer) : base(cellContainer)
        {
            AddToClassList(USSClasses.TableHeaderContainerVertical);
            cellContainer.horizontalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.left = offset;
        }
    }
}