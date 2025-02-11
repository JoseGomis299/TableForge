using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        public readonly int Id;
        public string Name;
        protected readonly TableControl TableControl;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                    RemoveFromClassList("selectedHeader");
                else
                    AddToClassList("selectedHeader");
                
                _isSelected = value;
            }
        }
        

        protected HeaderControl(int id, string name, TableControl tableControl)
        {
            Id = id;
            Name = name;
            TableControl = tableControl;
        }
    }
}