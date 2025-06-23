namespace TableForge.Editor.UI
{
    internal class  ColumnHeaderContainerControl : HeaderContainerControl
    {
        public ColumnHeaderContainerControl(TableControl tableControl) : base(tableControl)
        {
            AddToClassList(USSClasses.TableHeaderContainerHorizontal);
            if(tableControl.Parent != null)
            {
                AddToClassList(USSClasses.SubTableHeaderContainerHorizontal);
            }

            tableControl.ScrollView.verticalScroller.valueChanged += HandleOffset;
        }

        private void HandleOffset(float offset)
        {
            style.top = offset;
        }
    }
}