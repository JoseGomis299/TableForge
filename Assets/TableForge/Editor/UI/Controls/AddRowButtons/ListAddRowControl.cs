using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ListAddRowControl : AddRowControl
    {
        public ListAddRowControl(TableControl tableControl) : base(tableControl)
        {
            RowAdditionStrategy = new ListRowAdditionStrategy(TableControl);
            
            AddToClassList(USSClasses.AddRowButton);
            Label label = new Label("Add Row +");
            label.AddToClassList(USSClasses.AddRowButtonLabel);
            Add(label);
        }
    }
}