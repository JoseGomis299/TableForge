using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        public readonly int Id;
        public readonly string Name;
        
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


        protected HeaderControl(int id, string name, TableControl tableControl)
        {
            Id = id;
            Name = name;
            TableControl = tableControl;
        }
    }
}