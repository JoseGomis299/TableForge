using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CornerContainerControl : HeaderContainerControl
    {
        public TableCornerControl CornerControl => this[0] as TableCornerControl;
        public CornerContainerControl(TableControl tableControl) : base(tableControl)
        {
            AddToClassList(USSClasses.TableCornerContainer);
            if (tableControl.Parent != null)
            {
                AddToClassList(USSClasses.SubTableCornerContainer);
            }
            tableControl.ScrollView.horizontalScroller.valueChanged += HandleOffset;
            tableControl.ScrollView.verticalScroller.valueChanged += HandleVerticalOffset;
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