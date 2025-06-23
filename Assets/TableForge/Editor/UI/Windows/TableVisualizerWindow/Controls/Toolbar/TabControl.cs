using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class TabControl : VisualElement
    {
        private readonly ToolbarController _toolbarController;
        private TableMetadata _tableMetadata;
        
        private Button _selectButton;
        private Button _contextMenuButton;
     
        public TableMetadata TableMetadata => _tableMetadata;
        
        public TabControl(ToolbarController toolbarController, TableMetadata tableMetadata)
        {
            _toolbarController = toolbarController;
            _tableMetadata = tableMetadata;
            AddToClassList(USSClasses.ToolbarTab);
            
            _selectButton = new Button
            {
                name = "SelectButton"
            };
            _selectButton.AddToClassList(USSClasses.ToolbarTabSelectButton);
            _selectButton.clicked += OnSelectButtonClicked;
            
            _contextMenuButton = new Button
            {
                name = "ContextMenuButton"
            };
            _contextMenuButton.AddToClassList(USSClasses.ToolbarTabContextButton);
            _contextMenuButton.clicked += OnContextMenuButtonClicked;
            
            Add(_selectButton);
            Add(_contextMenuButton);
            ChangeButtonText(tableMetadata.Name);
            
            _toolbarController.OnEditionComplete += OnEditionComplete;
        }
        
        private void OnSelectButtonClicked()
        {
            _toolbarController.SelectTab(_tableMetadata);
        }
        
        private void OnContextMenuButtonClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Close"), false, () => _toolbarController.CloseTab(this));
            menu.AddItem(new GUIContent("Edit"), false, () =>
            {
                _toolbarController.EditTab(_tableMetadata);
            });
            menu.ShowAsContext();
        }
        
        private void OnEditionComplete(TableMetadata metadata)
        {
            if(metadata != _tableMetadata) return;
            ChangeButtonText(metadata.Name);
        }
        
        private void ChangeButtonText(string newText)
        {
            _selectButton.text = newText;
            float width = EditorStyles.label.CalcSize(new GUIContent(newText)).x + UiConstants.TabPadding + UiConstants.TabContextButtonWidth;
            width = Mathf.Clamp(width, UiConstants.TabMinWidth, UiConstants.TabMaxWidth);
            style.width = width;
        }
    }
}