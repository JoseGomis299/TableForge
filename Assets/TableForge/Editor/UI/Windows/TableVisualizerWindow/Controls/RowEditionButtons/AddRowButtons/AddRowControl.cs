using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class AddRowControl : Button
    {
        public event Action OnRowAdded;
        private readonly IRowAdditionStrategy _rowAdditionStrategy;
        private readonly TableControl _tableControl;

        public sealed override string text
        {
            get => base.text;
            set => base.text = value;
        }
        
        public AddRowControl(TableControl tableControl, IRowAdditionStrategy rowAdditionStrategy)
        {
            _rowAdditionStrategy = rowAdditionStrategy;
            _tableControl = tableControl;

            clicked += AddRow;
            AddToClassList(USSClasses.SubTableToolbarButton);
            text = "+";
        }

        private void AddRow()
        {
            _rowAdditionStrategy.AddRow(_tableControl);
            OnRowAdded?.Invoke();
        }
        
    }
}