using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SimpleCellControl : CellControl, ISimpleCellControl
    {
        private VisualElement _field;
        
        public VisualElement Field
        {
            get => _field;
            set
            {
                _field = value;
                _field.focusable = false;
                _field.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if(evt.button == (int)MouseButton.LeftMouse && !IsFieldFocused())
                    {
                        FocusField();
                    }
                }, TrickleDown.TrickleDown);
                _field.RegisterCallback<FocusOutEvent>(_ =>
                {
                   BlurField();
                });
            }
        }
        
        protected ISerializer Serializer { get; }
        
        protected SimpleCellControl(Cell cell, TableControl tableControl) : base(cell, tableControl)
        {
            Serializer = new JsonSerializer();
        }
        
        public virtual void FocusField()
        {
            if (Field == null) return;
            
            Field.focusable = true;
            Field.Focus();
        }
        
        public virtual void BlurField()
        {
            if (Field == null) return;
            
            Field.focusable = false;
            Field.Blur();
        }
        
        public bool IsFieldFocused()
        {
           return Field?.focusController?.focusedElement == Field;
        }
    }
}