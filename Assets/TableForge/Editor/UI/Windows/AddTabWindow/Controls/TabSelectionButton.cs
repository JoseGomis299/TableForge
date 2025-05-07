using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TabSelectionButton : VisualElement
    {
        private readonly TableMetadata _tableMetadata;
        private readonly AddTabViewModel _viewModel;
        private Button _mainButton;
        private Button _deleteButton;
        
        public TableMetadata TableMetadata => _tableMetadata;
        
        public TabSelectionButton(TableMetadata tableMetadata, AddTabViewModel viewModel)
        {
            _viewModel = viewModel;
            _tableMetadata = tableMetadata;
            AddToClassList(USSClasses.TabButtonContainer);
            
            _mainButton = new Button(OnClicked)
            {
                text = tableMetadata.Name,
                name = "main-button"
            };
            _mainButton.AddToClassList(USSClasses.TabButton);
            
            float width = EditorStyles.label.CalcSize(new GUIContent(tableMetadata.Name)).x + UiConstants.TabPadding;
            width = Mathf.Clamp(width, UiConstants.TabMinWidth, UiConstants.TabMaxWidth);
            _mainButton.style.width = width;
            
            _deleteButton = new Button(() => viewModel.DeleteTab(tableMetadata))
            {
                text = "X",
                name = "delete-button"
            };
            _deleteButton.AddToClassList(USSClasses.TabDeleteButton);
            
            Add(_mainButton);
            Add(_deleteButton);
            
            UpdateDeleteButtonVisibility();
        }

        private void OnClicked()
        {
            _viewModel.ToggleTab(this);
            UpdateDeleteButtonVisibility();
        }

        private void UpdateDeleteButtonVisibility()
        {
            if (_viewModel.IsTabOpen(_tableMetadata))
            {
                _deleteButton.style.display = DisplayStyle.None;
                _mainButton.RemoveFromClassList(USSClasses.TabButtonClosed);
            }
            else
            {
                _deleteButton.style.display = DisplayStyle.Flex;
                _mainButton.AddToClassList(USSClasses.TabButtonClosed);
            }
        }
    }
}