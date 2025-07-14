using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI.CustomControls
{
    internal class MultiSelectDropdownButton : ButtonBehaviorOverrider
    {
        private List<DropdownElement> _selectedItems = new();
        private List<DropdownElement> _allItems = new();
        private MultiSelectDropdownPopup _popupWindow;
        private bool _isBeingClicked;
        
        public Action<List<DropdownElement>> onSelectionChanged;

        public MultiSelectDropdownButton(Button button) : base(button)
        {
        }
        
        protected override void OnButtonClicked()
        {
            if(_allItems.Count == 0)
                return;

            if (_popupWindow != null && _popupWindow.IsOpen)
            {
                _popupWindow.Close();
                _popupWindow = null;
                return;
            }

            _popupWindow = MultiSelectDropdownPopup.Show(_allItems, _selectedItems, this, selected =>
            {
                if(selected.Count == _selectedItems.Count && selected.TrueForAll(item => _selectedItems.Contains(item)))
                    return;
                
                if(_selectedItems.SequenceEqual(selected))
                    return;
                
                _selectedItems = selected;
                onSelectionChanged?.Invoke(_selectedItems);
            });
        }

        
        public void SetItems(List<DropdownElement> items, List<DropdownElement> selectedItems)
        {
            _selectedItems = new List<DropdownElement>(selectedItems);
            _allItems = new List<DropdownElement>(items);
        }
    }
}