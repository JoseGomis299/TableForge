using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TabSelectionButton : Button
    {
        private readonly TableMetadata _tableMetadata;
        private readonly AddTabViewModel _viewModel;
        
        public TableMetadata TableMetadata => _tableMetadata;
        
        public sealed override string text
        {
            get => base.text;
            set => base.text = value;
        }
        
        public TabSelectionButton(TableMetadata tableMetadata, AddTabViewModel viewModel)
        {
            _viewModel = viewModel;
            _tableMetadata = tableMetadata;
            AddToClassList(USSClasses.TabButton);
            text = tableMetadata.Name;
            
            float width = EditorStyles.label.CalcSize(new GUIContent(tableMetadata.Name)).x + UiConstants.TabPadding;
            width = Mathf.Clamp(width, UiConstants.TabMinWidth, UiConstants.TabMaxWidth);
            style.width = width;
            
            clicked += OnClicked;
        }

        private void OnClicked()
        {
            _viewModel.ToggleTab(this);
        }
    }
}