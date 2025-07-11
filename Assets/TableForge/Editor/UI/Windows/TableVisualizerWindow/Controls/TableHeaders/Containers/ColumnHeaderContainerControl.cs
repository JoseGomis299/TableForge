using TableForge.Editor.UI.UssClasses;

namespace TableForge.Editor.UI
{
    internal class  ColumnHeaderContainerControl : HeaderContainerControl
    {
        public ColumnHeaderContainerControl(TableControl tableControl) : base(tableControl)
        {
            AddToClassList(TableVisualizerUss.TableHeaderContainerHorizontal);
            if(tableControl.Parent != null)
            {
                AddToClassList(TableVisualizerUss.SubTableHeaderContainerHorizontal);
            }

            tableControl.ScrollView.verticalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.top = offset;
        }
    }
}