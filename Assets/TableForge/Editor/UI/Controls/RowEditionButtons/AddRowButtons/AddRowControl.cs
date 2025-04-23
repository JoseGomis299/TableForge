using System;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class AddRowControl : VisualElement
    {
        public event Action OnRowAdded;
        private readonly IRowAdditionStrategy _rowAdditionStrategy;
        private readonly TableControl _tableControl;

        public AddRowControl(TableControl tableControl, IRowAdditionStrategy rowAdditionStrategy)
        {
            _rowAdditionStrategy = rowAdditionStrategy;
            _tableControl = tableControl;

            this.AddManipulator(new Clickable(AddRow));
            AddToClassList(USSClasses.SubTableToolbarButton);

            Label text = new Label("+");
            text.AddToClassList(USSClasses.RowEditionButtonLabel);
            Add(text);
        }
        
        private void AddRow()
        {
            _rowAdditionStrategy.AddRow(_tableControl);
            OnRowAdded?.Invoke();
        }
        
    }
}