using System;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class TextBasedCellControl<T> : SimpleCellControl, ITextBasedCellControl
    {

        private TextInputBaseField<T> _textField;
        
        public TextInputBaseField<T> TextField
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
            
            try
            {
                _textField.value = (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception)
            {
                return;
            }
            
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

        public string GetValue()
        {
            if (_textField == null) return string.Empty;
            
            var value = _textField.text;
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value;
        }

        protected override void OnRefresh()
        {
            if (_textField != null)
            {
                _textField.value = (T)Convert.ChangeType(Cell.GetValue(), typeof(T));
            }
        }
    }
}