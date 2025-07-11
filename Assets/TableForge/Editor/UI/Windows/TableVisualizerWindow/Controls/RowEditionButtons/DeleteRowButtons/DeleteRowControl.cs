using System;
using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class DeleteRowControl : Button
    {
        public event Action OnRowDeleted;
        
        private readonly TableControl _tableControl;
        private readonly IRowDeletionStrategy _rowDeletionStrategy;

        public sealed override string text
        {
            get => base.text;
            set => base.text = value;
        }

        public DeleteRowControl(TableControl tableControl, IRowDeletionStrategy rowDeletionStrategy)
        {
            _rowDeletionStrategy = rowDeletionStrategy;
            _tableControl = tableControl;
            
            clicked += DeleteRow;
            AddToClassList(TableVisualizerUss.SubTableToolbarButton);
            text = "-";
        }
        
        private void DeleteRow()
        {
            _rowDeletionStrategy.DeleteRow(_tableControl);
            OnRowDeleted?.Invoke();
        }
        
    }
}