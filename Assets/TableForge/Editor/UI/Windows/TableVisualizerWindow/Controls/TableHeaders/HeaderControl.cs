using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        public readonly TableControl TableControl;
        private bool _isSelected;
        
        public CellAnchor CellAnchor { get; protected set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                    RemoveFromClassList(USSClasses.SelectedHeader);
                else
                    AddToClassList(USSClasses.SelectedHeader);

                _isSelected = value;
            }
        }
        public bool IsVisible { get; set; }
        public string Name => CellAnchor?.Name ?? string.Empty;
        public string Id => CellAnchor?.Id ?? string.Empty;


        protected HeaderControl(CellAnchor cellAnchor, TableControl tableControl)
        {
            CellAnchor = cellAnchor;
            TableControl = tableControl;
            
            IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
            tableControl.CellSelector.OnSelectionChanged += () =>
            {
                IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
            };

            if (cellAnchor is Row && !tableControl.Metadata.IsTypeBound)
            {
                this.AddManipulator(new ContextualMenuManipulator(MenuBuilder));
            }
        }

        private void MenuBuilder(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("Remove Row", (a) =>
            {
                TableControl.RemoveRow(CellAnchor.Id);
            });
        }
    }
}