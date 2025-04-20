using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ListAddRowControl : AddRowControl
    {
        public ListAddRowControl(TableControl tableControl) : base(tableControl)
        {
            RowAdditionStrategy = new ListRowAdditionStrategy(TableControl);
            
            Label label = new Label("+");
            label.AddToClassList(USSClasses.AddRowButtonLabel);
            Add(label);
        }
    }
}