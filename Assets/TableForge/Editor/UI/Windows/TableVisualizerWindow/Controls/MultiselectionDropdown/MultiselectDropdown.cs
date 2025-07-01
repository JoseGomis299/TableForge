using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class MultiSelectDropdown 
    {
        private readonly Button _button;
        
        private List<DropdownElement> _selectedItems = new();
        private List<DropdownElement> _allItems = new();

        public Action<List<DropdownElement>> onSelectionChanged;

        public MultiSelectDropdown(List<DropdownElement> items, Button button)
        {
            _allItems = new List<DropdownElement>(items);
            button.clicked += OpenPopup;
            _button = button;
        }

        private void OpenPopup()
        {
            if(_button == null || _allItems.Count == 0 || MultiSelectDropdownPopup.IsOpen)
                return;
            
            var popupRect = _button.worldBound; 
            MultiSelectDropdownPopup.Show(_allItems, _selectedItems, popupRect, selected =>
            {
                if(selected.Count == _selectedItems.Count && selected.TrueForAll(item => _selectedItems.Contains(item)))
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