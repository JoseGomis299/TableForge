using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class NullItemAddRowControl : AddRowControl
    {
        public NullItemAddRowControl(TableControl tableControl) : base(tableControl)
        {
            RowAdditionStrategy = new NullItemRowAdditionStrategy(TableControl);
            
            AddToClassList(USSClasses.AddRowButton);
            Label label = new Label("Set Value +");
            label.AddToClassList(USSClasses.AddRowButtonLabel);
            Add(label);
            this.AddManipulator(new Clickable(AddRow));
        }

        protected override void AddRow()
        {
            base.AddRow();
            RemoveFromHierarchy();
        }
    }
}