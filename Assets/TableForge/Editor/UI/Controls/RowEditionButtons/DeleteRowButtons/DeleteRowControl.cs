using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class DeleteRowControl : VisualElement
    {
        public event Action OnRowDeleted;
        
        private readonly TableControl _tableControl;
        private readonly IRowDeletionStrategy _rowDeletionStrategy;

        public DeleteRowControl(TableControl tableControl, IRowDeletionStrategy rowDeletionStrategy)
        {
            _rowDeletionStrategy = rowDeletionStrategy;
            _tableControl = tableControl;
            
            this.AddManipulator(new Clickable(DeleteRow));
            AddToClassList(USSClasses.SubTableToolbarButton);

            Label text = new Label("-");
            text.AddToClassList(USSClasses.RowEditionButtonLabel);
            Add(text);
        }

        private void DeleteRow()
        {
            _rowDeletionStrategy.DeleteRow(_tableControl);
            OnRowDeleted?.Invoke();
        }
        
    }
}