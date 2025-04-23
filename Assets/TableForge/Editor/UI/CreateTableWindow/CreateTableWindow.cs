using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    public class CreateTableWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        
        private CreateTableViewModel _viewModel;
    

        [MenuItem("TableForge/Create Table Window")]
        public static void ShowWindow() => GetWindow<CreateTableWindow>("Create Table");

        public void CreateGUI()
        {
            rootVisualElement.Add(visualTreeAsset.Instantiate());
            _viewModel = new CreateTableViewModel();
            
            // Confirm Button
            var confirmBtn = rootVisualElement.Q<Button>(name: "confirm-button");
            confirmBtn.clicked += () => _viewModel.CreateTable();
            
            // Bind AssetTreeView
            var assetTreeContainer = rootVisualElement.Q<VisualElement>(name: "asset-tree-container");
            var assetTree = new AssetTreeView();
            assetTreeContainer.Add(assetTree);
            assetTree.OnItemSelectionChanged += (item, selected) =>
            {
                _viewModel.OnItemSelected(item, selected);
                UpdateConfirm(confirmBtn);
            };

            // Bind ModeSelector
            var modeSel = rootVisualElement.Q<RadioButtonGroup>(name: "mode-selector");
            modeSel.RegisterValueChangedCallback(evt =>
            {
                _viewModel.UsePathsMode = (evt.newValue == 1);
                RefreshTree(assetTree, assetTreeContainer);
                UpdateConfirm(confirmBtn);
            });
            modeSel.value = 0; // Default to types mode

            // Bind Type Dropdown
            var typeDropdown = rootVisualElement.Q<DropdownField>(name: "type-dropdown");
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                _viewModel.OnTypeDropdownValueChanged(evt);
                RefreshTree(assetTree, assetTreeContainer);
                UpdateConfirm(confirmBtn);
            });
            
            // Bind Namespace Dropdown
            var namespaceDropdown = rootVisualElement.Q<DropdownField>(name: "namespace-dropdown");
            namespaceDropdown.RegisterValueChangedCallback(evt =>
            {
                _viewModel.OnNamespaceDropdownValueChanged(evt, typeDropdown);
            });
            
            // Populate dropdowns
            _viewModel.PopulateNamespaceDropdown(namespaceDropdown);
            _viewModel.PopulateTypeDropdown(typeDropdown);
            
            // Initial state
            UpdateConfirm(confirmBtn);
        }
        
        
        void RefreshTree(AssetTreeView assetTree, VisualElement container)
        {
            if (_viewModel.UsePathsMode && _viewModel.SelectedType != null)
            {
                container.style.display = DisplayStyle.Flex;
                _viewModel.RefreshTree();
                assetTree.ItemsSource = _viewModel.TreeItems;
            }
            else if (!_viewModel.UsePathsMode)
            {
                container.style.display = DisplayStyle.None;
            }
        }
        
        void UpdateConfirm(Button confirmBtn) => confirmBtn.SetEnabled(_viewModel.CanConfirm());
    }
}