using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class AddTabWindow : EditorWindow
    {
        private static bool _isOpened;
        
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        private AddTabViewModel ViewModel { get; set; }

        // UI elements
        private VisualElement _openTabsContainer;
        private VisualElement _existingTablesContainer;
        private Button _clearTabsButton;
        private Button _createButton;
        private Button _cancelButton;
        private Button _confirmButton;
        
        private void OnDisable()
        {
            _isOpened = false;
            WindowManager.CloseModalWindow(this);
        }
        
        public static void ShowWindow(AddTabViewModel viewModel)
        {
            if (_isOpened) return;
            _isOpened = true;

            var wnd = CreateInstance<AddTabWindow>();
            wnd.titleContent = new GUIContent("Add Tabs");
            wnd.ViewModel = viewModel;
            wnd.minSize = new Vector2(550, 450);
            wnd.Initialize();
            WindowManager.ShowModalWindow(wnd);
        }

        private void Initialize()
        {
            rootVisualElement.Add(visualTreeAsset.Instantiate());

            _openTabsContainer = rootVisualElement.Q<VisualElement>("open-tabs-scrollview-content");
            _existingTablesContainer = rootVisualElement.Q<VisualElement>("existing-tables-scrollview-content");

            _clearTabsButton = rootVisualElement.Q<Button>("clear-tabs-button");
            _createButton = rootVisualElement.Q<Button>("create-button");
            _cancelButton = rootVisualElement.Q<Button>("cancel-button");
            _confirmButton = rootVisualElement.Q<Button>("accept-button");

            ViewModel.PopulateTabContainers(_existingTablesContainer, _openTabsContainer);
            BindEvents();
        }

        private void BindEvents()
        {
            _clearTabsButton.clicked += () =>
            {
                ViewModel?.ClearTabs();
                ViewModel?.UpdateTabContainers(_existingTablesContainer, _openTabsContainer);
            };
            _createButton.clicked += () => ViewModel?.CreateNewTable(_existingTablesContainer);
            _confirmButton.clicked += () =>
            {
                ViewModel?.AddCurrentTabs();
                WindowManager.CloseModalWindow(this);
                Close();
            };
            
            _cancelButton.clicked += () =>
            {
                WindowManager.CloseModalWindow(this);
                Close();
            };
            
            ViewModel.OnTabSelectionChanged += OnTabSelectionChanged;
        }

        private void OnTabSelectionChanged(TabSelectionButton tab, bool selected)
        {
            if (selected && _existingTablesContainer.Contains(tab))
            {
                _openTabsContainer.Add(tab);
            }
            else if (!selected && _openTabsContainer.Contains(tab))
            {
                _existingTablesContainer.Add(tab);
            }
        }
    }
}
