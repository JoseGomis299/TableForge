using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TextBasedCellControl<T> : SimpleCellControl, ITextBasedCellControl
    {

        private TextInputBaseField<T> _textField;
        
        protected TextInputBaseField<T> TextField
        {
            get => _textField;
            set
            {
                _textField = value;
                Field = value;
            }
        }
        
        public TextBasedCellControl(Cell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }

        public override void FocusField()
        {
            _textField?.schedule.Execute(() =>
            {
                if (_textField == null) return;
                _textField.focusable = true;
                _textField.tabIndex = 0;
            
                _textField.Focus();
                _textField.SelectAll();
            }).ExecuteLater(0);
        }
        
        public override void BlurField()
        {
            if(_textField == null) return;
            
            _textField.tabIndex = 0;
            _textField.cursorIndex = 0;
            _textField.SelectNone();
            
            _textField.focusable = false;
            _textField.Blur();
        }
        
        public void SetValue(string value, bool focus)
        {
            if (_textField == null) return;
            _textField.value = (T)Convert.ChangeType(value, typeof(T));
            
            if(focus)
            {
                _textField.schedule.Execute(() =>
                {
                    _textField.focusable = true;
                    _textField.tabIndex = 0;
                    
                    _textField.Focus();

                    _textField.cursorIndex = value.Length;
                    _textField.selectIndex = value.Length;
                }).ExecuteLater(0);
            }
        }
    }
}