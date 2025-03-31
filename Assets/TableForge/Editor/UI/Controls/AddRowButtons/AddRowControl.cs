using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class AddRowControl : VisualElement
    {
        public event Action OnRowAdded;
        
        protected IRowAdditionStrategy RowAdditionStrategy;
        protected readonly TableControl TableControl; 
        
        protected AddRowControl(TableControl tableControl)
        {
            TableControl = tableControl;
            
            this.AddManipulator(new Clickable(AddRow));
        }
        
        protected virtual void AddRow()
        {
            RowAdditionStrategy.AddRow();
            OnRowAdded?.Invoke();
        }
        
    }
}