using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class AddRowControl : VisualElement
    {
        protected IRowAdditionStrategy RowAdditionStrategy;
        protected readonly TableControl TableControl; 
        
        protected AddRowControl(TableControl tableControl)
        {
            TableControl = tableControl;
        }
        
        protected virtual void AddRow()
        {
            RowAdditionStrategy.AddRow();
        }
        
    }
}