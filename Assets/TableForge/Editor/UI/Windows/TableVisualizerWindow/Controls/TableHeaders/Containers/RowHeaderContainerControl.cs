using TableForge.Editor.UI.UssClasses;

namespace TableForge.Editor.UI
{
    internal class RowHeaderContainerControl : HeaderContainerControl
    {
        public RowHeaderContainerControl(TableControl tableControl) : base(tableControl)
        {
            AddToClassList(TableVisualizerUss.TableHeaderContainerVertical);
            tableControl.ScrollView.horizontalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.left = offset;
        }
    }
}