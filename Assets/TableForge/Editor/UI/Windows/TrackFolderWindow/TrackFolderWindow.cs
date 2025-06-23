using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class TrackFolderWindow : EditorWindow
    {
        private TextField _pathField;
        private DropdownField _pathDropdown;
        private Button _acceptButton;
        private Button _cancelButton;
        private string[] _existingPaths;

        private TableDetailsViewModel _detailsViewModel;
        private static bool _isOpened;

        public static void ShowWindow(TableDetailsViewModel viewModel)
        {
            if(_isOpened) return;
            _isOpened = true;
            
            var wnd = CreateInstance<TrackFolderWindow>();
            wnd.titleContent = new GUIContent("Track Folder");
            wnd._detailsViewModel = viewModel;
            wnd.minSize = new Vector2(800, 80);
            wnd.maxSize = new Vector2(999999, 80);
            wnd.Initialize();
            WindowManager.ShowModalWindow(wnd);
        }
        
        private void OnDisable()
        {
            _isOpened = false;
            WindowManager.CloseModalWindow(this);
        }

        private void Initialize()
        {
            // Load existing paths from Assets
            _existingPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && AssetDatabase.IsValidFolder(path))
                .ToArray();

            // Create UI
            var root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            // Path Field with Dropdown
            _pathField = new TextField("Folder Path") { value = "Assets/" };
            root.Add(_pathField);

            _pathDropdown = new DropdownField("Existing Paths", _existingPaths.ToList(), 0);
            _pathDropdown.RegisterValueChangedCallback(evt => _pathField.value = evt.newValue);
            root.Add(_pathDropdown);

            // Buttons
            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            _acceptButton = new Button(OnAcceptClicked) { text = "Accept" };
            _cancelButton = new Button(OnCancelClicked) { text = "Cancel" };
            buttonContainer.Add(_cancelButton);
            buttonContainer.Add(_acceptButton);
            root.Add(buttonContainer);
        }

        private void OnAcceptClicked()
        {
            string selectedPath = _pathField.value.Trim();
            if (string.IsNullOrEmpty(selectedPath)) return;

            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(selectedPath))
            {
                string parentPath = System.IO.Path.GetDirectoryName(selectedPath);
                string folderName = System.IO.Path.GetFileName(selectedPath);

                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    Debug.LogError($"Parent folder does not exist: {parentPath}");
                    return;
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
                AssetDatabase.Refresh();
            }

            // Add folder to tree view (logic depends on your implementation)
            if (_detailsViewModel != null)
            {
               _detailsViewModel.AddPathToTree(selectedPath);
            }

            Close();
        }

        private void OnCancelClicked()
        {
            Close();
        }
    }
}