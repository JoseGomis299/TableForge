using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class TableDetailsWindow<TViewModel> : EditorWindow where TViewModel : TableDetailsViewModel
    {
        private static bool _isOpened;

        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        protected TViewModel viewModel;
        
        // UI Elements
        private AssetTreeView _assetTreeView;
        private VisualElement _assetTreeContainer;
        private TextField _nameField;
        private RadioButtonGroup _modeSelector;
        private DropdownField _typeDropdown;
        private DropdownField _namespaceDropdown;
        private Label _errorText;
        private Button _confirmButton;
        private Button _trackFolderButton;

        protected static void ShowWindow<T>(TViewModel viewModel, string title) where T : TableDetailsWindow<TViewModel>
        {
            if (_isOpened) return;
            _isOpened = true;
            
            var wnd = CreateInstance<T>();
            wnd.titleContent = new GUIContent(title);
            wnd.viewModel = viewModel;
            wnd.minSize = new Vector2(320, 450);
            WindowManager.ShowModalWindow(wnd);
            wnd.Initialize();
        }

        protected abstract void OnConfirm();
        
        protected abstract string GetTableName();

        
        private void OnDisable()
        {
            _isOpened = false;
            WindowManager.CloseModalWindow(this);
        }
        
        private void Initialize()
        {
            rootVisualElement.Add(visualTreeAsset.Instantiate());
            
            FindElements();
            BindEvents();
            InitializeElements();

            viewModel.RefreshTree();
            UpdateState();
        }
        
        private void FindElements()
        {
            _errorText = rootVisualElement.Q<Label>(name: "error-text");
            _confirmButton = rootVisualElement.Q<Button>(name: "confirm-button");
            _nameField = rootVisualElement.Q<TextField>(name: "name-field");
            _assetTreeContainer = rootVisualElement.Q<VisualElement>(name: "asset-tree-container");
            _modeSelector = rootVisualElement.Q<RadioButtonGroup>(name: "mode-selector");
            _typeDropdown = rootVisualElement.Q<DropdownField>(name: "type-dropdown");
            _namespaceDropdown = rootVisualElement.Q<DropdownField>(name: "namespace-dropdown");
            _trackFolderButton = rootVisualElement.Q<Button>(name: "track-folder-button");
            
            _assetTreeView = new AssetTreeView(viewModel);
            _assetTreeContainer.Add(_assetTreeView);
        }

        private void BindEvents()
        {
            _confirmButton.clicked += OnConfirmButtonClicked;
            _trackFolderButton.clicked += OnTrackFolderButtonClicked;
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            
            _assetTreeView.OnItemSelectionChanged += OnTreeViewSelectionChanged;
            _assetTreeView.OnSelectionChanged += UpdateState;

            _modeSelector.RegisterValueChangedCallback(OnModeChanged);
            _typeDropdown.RegisterValueChangedCallback(OnTypeChanged);
            _namespaceDropdown.RegisterValueChangedCallback(OnNamespaceChanged);
            
            viewModel.OnTreeUpdated += RefreshTree; 
        }

        private void InitializeElements()
        {
            viewModel.PopulateNamespaceDropdown(_namespaceDropdown);
            viewModel.PopulateTypeDropdown(_typeDropdown);
            _nameField.value = GetTableName();
            viewModel.TableName = _nameField.value;
            _modeSelector.SetValueWithoutNotify(viewModel.UsePathsMode ? 1 : 0);
        }

        private void OnNamespaceChanged(ChangeEvent<string> evt)
        {
            viewModel.OnNamespaceDropdownValueChanged(evt, _typeDropdown);
            UpdateState();
        }

        private void OnTypeChanged(ChangeEvent<string> evt)
        {
            viewModel.OnTypeDropdownValueChanged(evt);
            viewModel.ClearSelectedAssets();
            viewModel.RefreshTree();

            _nameField.value = GetTableName();
            UpdateState();
        }

        private void OnModeChanged(ChangeEvent<int> evt)
        {
            viewModel.UsePathsMode = (evt.newValue == 1);
            viewModel.ClearSelectedAssets();
            viewModel.RefreshTree();
            UpdateState();
        }

        private void OnTreeViewSelectionChanged(TreeItem item, bool selected)
        {
            viewModel.OnItemSelected(item, selected);
            UpdateState();
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            viewModel.OnNameFieldValueChanged(evt, _nameField);
            UpdateState();
        }

        private void OnConfirmButtonClicked()
        {
            OnConfirm();
            WindowManager.CloseModalWindow(this);
            Close();
        }
        
        private void OnTrackFolderButtonClicked()
        {
            TrackFolderWindow.ShowWindow(viewModel);
        }

        private void RefreshTree()
        {
            if (viewModel.UsePathsMode && viewModel.SelectedType != null)
            {
                _assetTreeContainer.style.display = DisplayStyle.Flex;
                _assetTreeView.ItemsSource = viewModel.TreeItems;
            }
            else if (!viewModel.UsePathsMode)
            {
                _assetTreeContainer.style.display = DisplayStyle.None;
            }
        }

        private void UpdateState()
        {
            UpdateErrorText();
            _trackFolderButton.style.display = viewModel.UsePathsMode ? DisplayStyle.Flex : DisplayStyle.None;
            _confirmButton.SetEnabled(!viewModel.HasErrors);
        }
        
        private void UpdateErrorText()
        {
            string errorTxt = viewModel.GetErrors();
            
            if (string.IsNullOrEmpty(errorTxt))
            {
                _errorText.style.display = DisplayStyle.None;
            }
            else
            {
                _errorText.text = errorTxt;
                _errorText.style.display = DisplayStyle.Flex;
            }
        }
    }
}