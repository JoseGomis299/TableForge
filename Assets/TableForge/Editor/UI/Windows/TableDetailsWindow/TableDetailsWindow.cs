using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class TableDetailsWindow<TViewModel> : EditorWindow where TViewModel : TableDetailsViewModel
    {
        private static bool _isOpened;

        [SerializeField] protected VisualTreeAsset visualTreeAsset;
        protected TViewModel ViewModel;
        
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
            wnd.ViewModel = viewModel;
            WindowManager.ShowModalWindow(wnd);
            wnd.Initialize();
        }
        
        protected virtual void FindElements()
        {
            _errorText = rootVisualElement.Q<Label>(name: "error-text");
            _confirmButton = rootVisualElement.Q<Button>(name: "confirm-button");
            _nameField = rootVisualElement.Q<TextField>(name: "name-field");
            _assetTreeContainer = rootVisualElement.Q<VisualElement>(name: "asset-tree-container");
            _modeSelector = rootVisualElement.Q<RadioButtonGroup>(name: "mode-selector");
            _typeDropdown = rootVisualElement.Q<DropdownField>(name: "type-dropdown");
            _namespaceDropdown = rootVisualElement.Q<DropdownField>(name: "namespace-dropdown");
            _trackFolderButton = rootVisualElement.Q<Button>(name: "track-folder-button");
            
            _assetTreeView = new AssetTreeView(ViewModel);
            _assetTreeContainer.Add(_assetTreeView);
        }

        protected virtual void BindEvents()
        {
            _confirmButton.clicked += OnConfirmButtonClicked;
            _trackFolderButton.clicked += OnTrackFolderButtonClicked;
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            
            _assetTreeView.OnItemSelectionChanged += OnTreeViewSelectionChanged;
            _assetTreeView.OnSelectionChanged += UpdateState;

            _modeSelector.RegisterValueChangedCallback(OnModeChanged);
            _typeDropdown.RegisterValueChangedCallback(OnTypeChanged);
            _namespaceDropdown.RegisterValueChangedCallback(OnNamespaceChanged);
            
            ViewModel.OnTreeUpdated += RefreshTree; 
        }
        
        protected virtual void InitializeElements()
        {
            ViewModel.PopulateNamespaceDropdown(_namespaceDropdown);
            ViewModel.PopulateTypeDropdown(_typeDropdown);
            _nameField.value = GetTableName();
            ViewModel.TableName = _nameField.value;
            _modeSelector.SetValueWithoutNotify(ViewModel.UsePathsMode ? 1 : 0);
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

            ViewModel.RefreshTree();
            UpdateState();
        }

        private void OnNamespaceChanged(ChangeEvent<string> evt)
        {
            ViewModel.OnNamespaceDropdownValueChanged(evt, _typeDropdown);
            UpdateState();
        }

        private void OnTypeChanged(ChangeEvent<string> evt)
        {
            ViewModel.OnTypeDropdownValueChanged(evt);
            ViewModel.ClearSelectedAssets();
            ViewModel.RefreshTree();

            _nameField.value = GetTableName();
            UpdateState();
        }

        private void OnModeChanged(ChangeEvent<int> evt)
        {
            ViewModel.UsePathsMode = (evt.newValue == 1);
            ViewModel.ClearSelectedAssets();
            ViewModel.RefreshTree();
            UpdateState();
        }

        private void OnTreeViewSelectionChanged(TreeItem item, bool selected)
        {
            ViewModel.OnItemSelected(item, selected);
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            ViewModel.OnNameFieldValueChanged(evt, _nameField);
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
            TrackFolderWindow.ShowWindow(ViewModel);
        }

        private void RefreshTree()
        {
            if (ViewModel.UsePathsMode && ViewModel.SelectedType != null)
            {
                _assetTreeContainer.style.display = DisplayStyle.Flex;
                _assetTreeView.ItemsSource = ViewModel.TreeItems;
            }
            else if (!ViewModel.UsePathsMode)
            {
                _assetTreeContainer.style.display = DisplayStyle.None;
            }
        }

        private void UpdateState()
        {
            UpdateErrorText();
            _trackFolderButton.style.display = ViewModel.UsePathsMode ? DisplayStyle.Flex : DisplayStyle.None;
            _confirmButton.SetEnabled(!ViewModel.HasErrors);
        }
        
        private void UpdateErrorText()
        {
            string errorTxt = ViewModel.GetErrors();
            
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