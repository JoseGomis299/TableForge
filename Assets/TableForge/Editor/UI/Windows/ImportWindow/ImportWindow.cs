using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableForge.Editor.Serialization;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor
{
    public class ImportWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        
        private ImportViewModel _viewModel;
        private SerializationFormat _format = SerializationFormat.Csv;
        private bool _csvHasHeader = true;
        private string _selectedNamespace;
        private string _importedFileContent;

        private VisualElement _root;
        
        // UI Elements
        private TextField _tableNameField;
        private TextField _basePathField;
        private TextField _baseNameField;
        private EnumField _formatField;
        private DropdownField _namespaceDropdown;
        private DropdownField _typeDropdown;
        private Toggle _csvHeaderToggle;
        private TextField _dataInfoTextField;
        private TextField _dataPreviewTextField;
        private Button _importFileButton;
        private Button _acceptButton;
        private Button _cancelButton;
        private ListView _columnMappingListView;
        private ListView _itemReviewListView;
        private Label _errorLabel;
        
        private VisualElement _columnMappingContainer;
        private VisualElement _itemReviewContainer;
        private VisualElement _dataProcessingContainer;

        [MenuItem("Window/TableForge/Import Table")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportWindow>();
            window.titleContent = new GUIContent("Table Importer");
            window.minSize = new Vector2(600, 500);
        }

        public void CreateGUI()
        {
            _viewModel = new ImportViewModel();
            
            // Load UXML
            _root = visualTreeAsset.Instantiate();
            rootVisualElement.Add(_root);
            
            // Query elements
            _tableNameField = _root.Q<TextField>("table-name-field");
            _basePathField = _root.Q<TextField>("base-path-field");
            _baseNameField = _root.Q<TextField>("base-name-field");
            _formatField = _root.Q<EnumField>("format-field");
            _namespaceDropdown = _root.Q<DropdownField>("namespace-dropdown");
            _typeDropdown = _root.Q<DropdownField>("type-dropdown");
            _csvHeaderToggle = _root.Q<Toggle>("csv-header-toggle");
            _dataPreviewTextField = _root.Q<TextField>("data-preview-text-field");
            _dataInfoTextField = _root.Q<TextField>("data-info-text-field");
            _importFileButton = _root.Q<Button>("import-file-button");
            _errorLabel = _root.Q<Label>("error-label");
            _acceptButton = _root.Q<Button>("accept-button");
            _cancelButton = _root.Q<Button>("cancel-button");
            _columnMappingContainer = _root.Q<VisualElement>("column-mapping-container");
            _itemReviewContainer = _root.Q<VisualElement>("item-review-container");
            _dataProcessingContainer = _root.Q<VisualElement>("data-processing-container");
            
            // Create dynamic list views
            _columnMappingListView = _columnMappingContainer.Q<VisualElement>("column-mapping-list-container")[0] as ListView;
            _itemReviewListView = _itemReviewContainer.Q<VisualElement>("item-review-list-container")[0] as ListView;
            CreateColumnMappingListView();
            CreateItemReviewListView();
            
            // Initialize
            _format = SerializationFormat.Csv;
            _formatField.Init(_format);
            _csvHeaderToggle.style.display = DisplayStyle.Flex;
            _basePathField.value = "Assets/";
            _baseNameField.value = "NewElement";
            ShowProcessing();
            PopulateDropdowns();

            // Register callbacks
            _formatField.RegisterValueChangedCallback(evt => 
            {
                _format = (SerializationFormat)evt.newValue;
                _csvHeaderToggle.style.display = _format == SerializationFormat.Csv 
                    ? DisplayStyle.Flex 
                    : DisplayStyle.None;
            });
            
            _csvHeaderToggle.RegisterValueChangedCallback(evt => _csvHasHeader = evt.newValue);
            _importFileButton.clicked += ImportFile;
        }

        private void ShowProcessing()
        {
            _columnMappingContainer.style.display = DisplayStyle.None;
            _itemReviewContainer.style.display = DisplayStyle.None;
            _dataProcessingContainer.style.display = DisplayStyle.Flex;
            
            _acceptButton.clickable = new Clickable(ProcessData);
            
            _cancelButton.style.visibility = Visibility.Hidden;
            _acceptButton.text = "Process Data";
        }
        
        private void ShowMapping()
        {
            _columnMappingContainer.style.display = DisplayStyle.Flex;
            _itemReviewContainer.style.display = DisplayStyle.None;
            _dataProcessingContainer.style.display = DisplayStyle.None;
            
            _columnMappingListView.itemsSource = _viewModel.ColumnMappings;
            _columnMappingListView.Rebuild();
            
            _acceptButton.clickable = new Clickable(ShowItemReview);
            _cancelButton.clickable = new Clickable(ShowProcessing);
            
            _cancelButton.style.visibility = Visibility.Visible;
            _acceptButton.text = "Review Items";
            _cancelButton.text = "Back to Processing";
        }
        
        private void ShowItemReview()
        {
            try
            {
                _viewModel.ApplyColumnMappings();
                _viewModel.PrepareImportItems();
                
                // Show item review
                _itemReviewContainer.style.display = DisplayStyle.Flex;
                _itemReviewListView.itemsSource = _viewModel.ImportItems;
                _itemReviewListView.Rebuild();

                _dataProcessingContainer.style.display = DisplayStyle.None;
                _columnMappingContainer.style.display = DisplayStyle.None;
                
                _acceptButton.clickable = new Clickable(FinalizeImport);
                _acceptButton.text = "Finalize Import";
                _cancelButton.style.visibility = Visibility.Visible;
                _cancelButton.clickable = new Clickable(ShowMapping);
                _cancelButton.text = "Back to Mapping";
            }
            catch (Exception e)
            {
                ShowError($"Error applying mappings: {e.Message}");
            }
            
        }

        private void CreateColumnMappingListView()
        {
            _columnMappingListView.makeItem = () => new ColumnMappingItem();
            _columnMappingListView.bindItem = (element, index) =>
            {
                if (index >= _viewModel.ColumnMappings.Count) return;
                var item = (ColumnMappingItem)element;
                item.Bind(_viewModel.ColumnMappings[index], _viewModel.AvailableFields);
            };
            _columnMappingListView.selectionType = SelectionType.None;
        }

        private void CreateItemReviewListView()
        {
            _itemReviewListView.makeItem = () => new ItemReviewItem(_viewModel);
            _itemReviewListView.bindItem = (element, index) =>
            {
                if (index >= _viewModel.ImportItems.Count) return;
                var item = (ItemReviewItem)element;
                item.Bind(_viewModel.ImportItems[index], _viewModel.ItemsType);
            };
            _itemReviewListView.fixedItemHeight = 60; 
            _itemReviewListView.selectionType = SelectionType.None;
    
            // Ensure proper layout
            var listContainer = _itemReviewContainer.Q<VisualElement>("item-review-list-container");
            listContainer.style.flexGrow = 1;
            _itemReviewListView.style.flexGrow = 1;
        }
        
        private void ImportFile()
        {
            string extension = _format == SerializationFormat.Csv ? "csv" : "json";
            string path = EditorUtility.OpenFilePanel("Import Data", "", extension);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                _importedFileContent = File.ReadAllText(path);
                bool fileIsTooLarge = _importedFileContent.Length > 10000; 
                string previewContent = fileIsTooLarge 
                    ? "(Truncated)\n\n"+_importedFileContent.Substring(0, 10000) + "..." 
                    : "\n"+_importedFileContent;
                _dataPreviewTextField.value = $"Imported file: {Path.GetFileName(path)}\n\n" +
                                              $"Content Preview:\n{previewContent}";
            }
            catch (Exception e)
            {
                ShowError($"Error reading file: {e.Message}");
            }
        }

        private void ProcessData()
        {
            ClearError();
            
            // Validate inputs
            if (!ValidateProcessingInput()) return;

            // Process data
            try
            {
                _viewModel.TableName = _tableNameField.value;
                _viewModel.Format = _format;
                _viewModel.CsvHasHeader = _csvHasHeader;
                _viewModel.Data = _importedFileContent;
                _viewModel.NewElementsBasePath = _basePathField.value;
                _viewModel.NewElementsBaseName = _baseNameField.value;
                
                _viewModel.ItemsType = TypeRegistry.TypesByNamespaceAndName[_namespaceDropdown.value][_typeDropdown.value];
                _viewModel.ProcessData();
                _dataInfoTextField.value = _viewModel.GetDataInfo();

                ShowMapping();
            }
            catch (Exception e)
            {
                ShowError($"Error processing data: {e.Message}");
            }
        }

        private bool ValidateProcessingInput()
        {
            if (string.IsNullOrWhiteSpace(_tableNameField.value))
            {
                ShowError("Table name is required.");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(_importedFileContent))
            {
                ShowError("File is required.");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(_typeDropdown.value))
            {
                ShowError("Please select a data type.");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(_basePathField.value))
            {
                ShowError("Base path is required.");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(_baseNameField.value))
            {
                ShowError("Base name is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(_tableNameField.value) || TableMetadataManager.LoadMetadata(_tableNameField.value) != null)
            {
                ShowError($"Table name '{_tableNameField.value}' already exists or is invalid.");
                return false;
            }

            return true;
        }

    
        
        private void PopulateDropdowns()
        {
            _namespaceDropdown.choices.Clear();
            _namespaceDropdown.SetValueWithoutNotify(string.Empty);
            
            if (string.IsNullOrEmpty(_selectedNamespace) || !TypeRegistry.Namespaces.Contains(_selectedNamespace))
            {
                _selectedNamespace = TypeRegistry.Namespaces.FirstOrDefault();
            }

            _namespaceDropdown.choices = TypeRegistry.Namespaces.ToList();
            _namespaceDropdown.SetValueWithoutNotify(_selectedNamespace);
            
            _namespaceDropdown.RegisterValueChangedCallback(evt =>
            {
                if (TypeRegistry.NamespaceTypes.ContainsKey(evt.newValue))
                {
                    PopulateTypeDropdown(TypeRegistry.NamespaceTypes[evt.newValue]);
                }
            });
            
            if(string.IsNullOrEmpty(_selectedNamespace)) return;
            if (TypeRegistry.NamespaceTypes.ContainsKey(_selectedNamespace))
            {
                PopulateTypeDropdown(TypeRegistry.NamespaceTypes[_selectedNamespace]);
            }
        }

        private void PopulateTypeDropdown(HashSet<Type> types)
        {
            var typeNames = types.OrderBy(t => t.Name).Select(t => t.Name).ToList();
            _typeDropdown.choices = typeNames;
            if (typeNames.Count > 0)
            {
                _typeDropdown.value = typeNames[0];
            }
        }

        private void FinalizeImport()
        {
            try
            {
                _viewModel.FinalizeImport();
                Close();
                EditorUtility.DisplayDialog("Import Successful", "Table imported successfully!", "OK");
            }
            catch (Exception e)
            {
                ShowError($"Error during import: {e.Message}");
            }
        }

        private void ShowError(string message)
        {
            _errorLabel.text = message;
            _errorLabel.style.display = DisplayStyle.Flex;
        }

        private void ClearError()
        {
            _errorLabel.style.display = DisplayStyle.None;
        }
    }
}