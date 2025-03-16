using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        public CellAnchor CellAnchor { get; protected set; }
        

        protected readonly TableControl TableControl;

        private bool _isSelected;
        
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
        public int Id => CellAnchor?.Id ?? 0;


        protected HeaderControl(CellAnchor cellAnchor, TableControl tableControl)
        {
            CellAnchor = cellAnchor;
            TableControl = tableControl;
            
            tableControl.CellSelector.OnSelectionChanged += () =>
            {
                IsSelected = tableControl.CellSelector.SelectedAnchors.Contains(CellAnchor);
            };
        }
    }
}