using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowHeaderContainerControl : HeaderContainerControl
    {
        public RowHeaderContainerControl(TableControl tableControl) : base(tableControl)
        {
            AddToClassList(USSClasses.TableHeaderContainerVertical);
            tableControl.ScrollView.horizontalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.left = offset;
        }
    }
}