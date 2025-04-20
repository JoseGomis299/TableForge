using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class NullItemAddRowControl : AddRowControl
    {
        public NullItemAddRowControl(TableControl tableControl) : base(tableControl)
        {
            RowAdditionStrategy = new NullItemRowAdditionStrategy(TableControl);
            
            Label label = new Label("+");
            label.AddToClassList(USSClasses.AddRowButtonLabel);
            Add(label);
        }

        protected override void AddRow()
        {
            base.AddRow();
            RemoveFromHierarchy();
        }
    }
}