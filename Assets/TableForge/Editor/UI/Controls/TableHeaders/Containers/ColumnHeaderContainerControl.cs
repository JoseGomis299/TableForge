using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class  ColumnHeaderContainerControl : HeaderContainerControl
    {
        public ColumnHeaderContainerControl(ScrollView cellContainer) : base(cellContainer)
        {
            AddToClassList(USSClasses.TableHeaderContainerHorizontal);
            cellContainer.verticalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.top = offset;
        }
    }
}