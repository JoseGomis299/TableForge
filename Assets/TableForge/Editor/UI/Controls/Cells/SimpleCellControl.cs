using System;
using TableForge.Exceptions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SimpleCellControl : CellControl, ISimpleCellControl
    {
        public event Action<object> OnValueChange; 
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
        
        protected void OnChange<T>(ChangeEvent<T> evt, BaseField<T> field)
        {
            try
            {
                SetCellValue(evt.newValue);
                OnValueChange?.Invoke(evt.newValue);
            }
            catch(InvalidCellValueException e)
            {
                field.SetValueWithoutNotify(evt.previousValue);
                Debug.LogWarning(e.Message);
            }
            finally
            {
                RecalculateSize();
            }
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