using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class MultiSelectDropdown : VisualElement
    {
        private readonly Label _label;
        private readonly Button _button;

        private List<DropdownElement> _selectedItems = new();
        private List<DropdownElement> _allItems = new();

        public Action<List<DropdownElement>> OnSelectionChanged;

        public MultiSelectDropdown(List<DropdownElement> items, string text)
        {
            _allItems = new List<DropdownElement>(items);

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.borderBottomWidth = 1;
            style.borderTopWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;

            _label = new Label(text);
            _label.style.flexGrow = 1;

            _button = new Button(OpenPopup)
            {
                text = "▼",
                style = { width = 20, height = 18 }
            };

            Add(_label);
            Add(_button);
        }

        private void OpenPopup()
        {
            var popupRect = worldBound; 
            MultiSelectDropdownPopup.Show(_allItems, _selectedItems, popupRect, selected =>
            {
                if(selected.Count == _selectedItems.Count && selected.TrueForAll(item => _selectedItems.Contains(item)))
                    return;
                
                _selectedItems = selected;
                OnSelectionChanged?.Invoke(_selectedItems);
            });
        }

        
        public void SetItems(List<DropdownElement> items, List<DropdownElement> selectedItems)
        {
            _selectedItems = new List<DropdownElement>(selectedItems);
            _allItems = new List<DropdownElement>(items);
        }
    }
}