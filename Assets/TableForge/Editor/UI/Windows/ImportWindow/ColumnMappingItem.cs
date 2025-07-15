using System.Collections.Generic;
using UnityEngine.UIElements;
using TableForge.Editor.UI.UssClasses;

namespace TableForge.Editor
{
    internal class ColumnMappingItem : VisualElement
    {
        public Label Label { get; }
        public DropdownField Dropdown { get; }
        
        private ColumnMapping _mapping;

        public ColumnMappingItem()
        {
            AddToClassList(ImportUss.ColumnMappingItem);

            Label = new Label();
            Label.AddToClassList(ImportUss.ColumnMappingItemLabel);
            Add(Label);

            Dropdown = new DropdownField();
            Dropdown.AddToClassList(ImportUss.ColumnMappingItemDropdown);
            Add(Dropdown);
            
            Dropdown.RegisterValueChangedCallback(OnDropdownChange);
        }

        public void Bind(ColumnMapping mapping, List<string> availableFields)
        {
            Label.text = !string.IsNullOrEmpty(mapping.ColumnLetter)
                ? $"{mapping.ColumnLetter}: {mapping.OriginalName}"
                : mapping.OriginalName;

            _mapping = mapping;
            Dropdown.choices = availableFields;
            Dropdown.SetValueWithoutNotify(mapping.MappedField);
        }
        
        void OnDropdownChange(ChangeEvent<string> evt)
        {
            _mapping.MappedField = evt.newValue;
        }
    }
}
