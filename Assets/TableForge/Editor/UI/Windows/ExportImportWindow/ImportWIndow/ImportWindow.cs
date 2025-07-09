using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.CM.Common;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
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

        private VisualElement _root;
        
        // UI Elements
        private TextField _tableNameField;
        private EnumField _formatField;
        private DropdownField _namespaceDropdown;
        private DropdownField _typeDropdown;
        private Toggle _csvHeaderToggle;
        private TextField _dataTextField;
        private TextField _dataPreviewTextField;
        private Button _importFileButton;
        private Button _processDataButton;
        private Button _backToProcessingButton;
        private Button _backToMappingButton;
        private Button _confirmMappingButton;
        private ListView _columnMappingListView;
        private ListView _itemReviewListView;
        private Button _finalImportButton;
        private Label _errorLabel;
        
        private VisualElement _columnMappingContainer;
        private VisualElement _itemReviewContainer;
        private VisualElement _dataProcessingContainer;

        [MenuItem("Window/TableForge/Import Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportWindow>();
            window.titleContent = new GUIContent("Import Table");
            window.minSize = new Vector2(600, 700);
        }

        public void CreateGUI()
        {
            _viewModel = new ImportViewModel();
            
            // Load UXML
            _root = visualTreeAsset.Instantiate();
            rootVisualElement.Add(_root);
            
            // Query elements
            _tableNameField = _root.Q<TextField>("table-name-field");
            _formatField = _root.Q<EnumField>("format-field");
            _namespaceDropdown = _root.Q<DropdownField>("namespace-dropdown");
            _typeDropdown = _root.Q<DropdownField>("type-dropdown");
            _csvHeaderToggle = _root.Q<Toggle>("csv-header-toggle");
            _dataTextField = _root.Q<TextField>("data-text-field");
            _dataPreviewTextField = _root.Q<TextField>("data-preview-text-field");
            _importFileButton = _root.Q<Button>("import-file-button");
            _processDataButton = _root.Q<Button>("process-data-button");
            _backToProcessingButton = _root.Q<Button>("cancel-mapping-button");
            _backToMappingButton = _root.Q<Button>("cancel-import-button");
            _confirmMappingButton = _root.Q<Button>("confirm-mapping-button");
            _errorLabel = _root.Q<Label>("error-label");
            _finalImportButton = _root.Q<Button>("final-import-button");
            
            _columnMappingContainer = _root.Q<VisualElement>("column-mapping-container");
            _itemReviewContainer = _root.Q<VisualElement>("item-review-container");
            _dataProcessingContainer = _root.Q<VisualElement>("data-processing-container");
            
            // Create dynamic list views
            _columnMappingListView = CreateColumnMappingListView();
            _itemReviewListView = CreateItemReviewListView();
            
            _columnMappingContainer.Q<VisualElement>("column-mapping-list-container").Add(_columnMappingListView);
            _itemReviewContainer.Q<VisualElement>("item-review-list-container").Add(_itemReviewListView);
            
            // Initialize
            _format = SerializationFormat.Csv;
            _formatField.Init(_format);
            _csvHeaderToggle.style.display = DisplayStyle.Flex;
            _columnMappingContainer.style.display = DisplayStyle.None;
            _itemReviewContainer.style.display = DisplayStyle.None;
            
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
            _processDataButton.clicked += ProcessData;
            _finalImportButton.clicked += FinalizeImport;
            _confirmMappingButton.clicked += ShowItemReview;
            
            _backToMappingButton.clicked += () => 
            {
                _columnMappingContainer.style.display = DisplayStyle.Flex;
                _itemReviewContainer.style.display = DisplayStyle.None;
                _dataProcessingContainer.style.display = DisplayStyle.None;
            };
            
            _backToProcessingButton.clicked += () => 
            {
                _columnMappingContainer.style.display = DisplayStyle.None;
                _itemReviewContainer.style.display = DisplayStyle.None;
                _dataProcessingContainer.style.display = DisplayStyle.Flex;
            };
            
            
            PopulateDropdowns();
        }

     private ListView CreateColumnMappingListView()
    {
        var listView = new ListView
        {
            makeItem = () => {
                var container = new VisualElement();
                container.AddToClassList("column-mapping-item");
                
                var label = new Label();
                label.AddToClassList("column-mapping-item__label");
                container.Add(label);
                
                var dropdown = new DropdownField();
                dropdown.AddToClassList("column-mapping-item__dropdown");
                container.Add(dropdown);
                
                return container;
            },
            bindItem = (element, index) =>
            {
                if (index >= _viewModel.ColumnMappings.Count) return;
                
                var mapping = _viewModel.ColumnMappings[index];
                var label = element.Q<Label>(className: "column-mapping-item__label");
                var dropdown = element.Q<DropdownField>(className: "column-mapping-item__dropdown");
                
                label.text = !string.IsNullOrEmpty(mapping.ColumnLetter) 
                    ? $"{mapping.ColumnLetter}: {mapping.OriginalName}" 
                    : mapping.OriginalName;
                
                dropdown.choices = _viewModel.AvailableFields;
                dropdown.value = mapping.MappedField;
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    mapping.MappedField = evt.newValue;
                });
            },
            selectionType = SelectionType.None
        };
        
        listView.AddToClassList("list-container");
        return listView;
    }

    private ListView CreateItemReviewListView()
    {
        var listView = new ListView
        {
            makeItem = () => {
                var container = new VisualElement();
                container.AddToClassList("item-review-item");
                
                // Path row
                var pathRow = new VisualElement();
                pathRow.AddToClassList("item-review-item__row");
                
                var pathLabel = new Label("Path:");
                pathLabel.AddToClassList("item-review-item__label");
                pathRow.Add(pathLabel);
                
                var pathField = new TextField();
                pathField.AddToClassList("item-review-item__path-field");
                pathRow.Add(pathField);
                
                container.Add(pathRow);
                
                // Asset row
                var assetRow = new VisualElement();
                assetRow.AddToClassList("item-review-item__row");
                
                var assetLabel = new Label("Asset:");
                assetLabel.AddToClassList("item-review-item__label");
                assetRow.Add(assetLabel);
                
                var objectField = new ObjectField();
                objectField.objectType = typeof(ScriptableObject);
                objectField.allowSceneObjects = false;
                objectField.AddToClassList("item-review-item__object-field");
                assetRow.Add(objectField);
                
                var statusLabel = new Label();
                statusLabel.AddToClassList("item-review-item__status-label");
                assetRow.Add(statusLabel);
                
                container.Add(assetRow);
                
                return container;
            },
            bindItem = (element, index) =>
            {
                if (index >= _viewModel.ImportItems.Count) return;
                
                var item = _viewModel.ImportItems[index];
                var pathField = element.Q<TextField>(className: "item-review-item__path-field");
                var objectField = element.Q<ObjectField>(className: "item-review-item__object-field");
                var statusLabel = element.Q<Label>(className: "item-review-item__status-label");
                
                pathField.value = item.Path;
                pathField.RegisterValueChangedCallback(evt => item.Path = evt.newValue);
                
                objectField.value = item.ExistingAsset;
                objectField.RegisterValueChangedCallback(evt => 
                {
                    item.ExistingAsset = (ScriptableObject)evt.newValue;
                    item.Guid = evt.newValue != null ? 
                        AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue)) : 
                        string.Empty;
                    item.Path = evt.newValue != null ? 
                        AssetDatabase.GetAssetPath(evt.newValue) : 
                        item.OriginalPath;
                    _viewModel.ValidateItems();
                    
                    // Update status
                    statusLabel.text = item.WillCreateNew ? "New Asset" : "Existing Asset";
                    statusLabel.RemoveFromClassList("item-review-item__status-label--new");
                    statusLabel.RemoveFromClassList("item-review-item__status-label--existing");
                    statusLabel.AddToClassList(item.WillCreateNew ? 
                        "item-review-item__status-label--new" : 
                        "item-review-item__status-label--existing");
                });
                
                statusLabel.text = item.WillCreateNew ? "New Asset" : "Existing Asset";
                statusLabel.RemoveFromClassList("item-review-item__status-label--new");
                statusLabel.RemoveFromClassList("item-review-item__status-label--existing");
                statusLabel.AddToClassList(item.WillCreateNew ? 
                    "item-review-item__status-label--new" : 
                    "item-review-item__status-label--existing");
            },
            selectionType = SelectionType.None
        };
        
        listView.AddToClassList("list-container");
        return listView;
    }
        private void ImportFile()
        {
            string extension = _format == SerializationFormat.Csv ? "csv" : "json";
            string path = EditorUtility.OpenFilePanel("Import Data", "", extension);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                _dataTextField.value = File.ReadAllText(path);
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
            if (string.IsNullOrWhiteSpace(_tableNameField.value))
            {
                ShowError("Table name is required.");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_dataTextField.value))
            {
                ShowError("Data input is required.");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_typeDropdown.value))
            {
                ShowError("Please select a data type.");
                return;
            }
            
            if (string.IsNullOrEmpty(_tableNameField.value) || TableMetadataManager.LoadMetadata(_tableNameField.value) != null)
            {
                ShowError($"Table name '{_tableNameField.value}' already exists or is invalid.");
                return;
            }
            
            // Process data
            try
            {
                _viewModel.TableName = _tableNameField.value;
                _viewModel.Format = _format;
                _viewModel.CsvHasHeader = _csvHasHeader;
                _viewModel.Data = _dataTextField.value;
                
                //Set preview for the next step
                _dataPreviewTextField.value = _viewModel.Data;
                
                // Get selected type
                Type selectedType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == _typeDropdown.value && t.IsSubclassOf(typeof(ScriptableObject)));
                
                if (selectedType == null)
                {
                    ShowError("Selected type not found.");
                    return;
                }
                
                _viewModel.ItemsType = selectedType;
                _viewModel.ProcessData();
                
                // Show column mapping
                _dataProcessingContainer.style.display = DisplayStyle.None;
                _columnMappingContainer.style.display = DisplayStyle.Flex;
                _columnMappingListView.style.display = DisplayStyle.Flex;
                _columnMappingListView.itemsSource = _viewModel.ColumnMappings;
                _columnMappingListView.Rebuild();
            }
            catch (Exception e)
            {
                ShowError($"Error processing data: {e.Message}");
            }
        }

        private void ShowItemReview()
        {
            // Apply column mappings
            try
            {
                _viewModel.ApplyColumnMappings();
                _viewModel.PrepareImportItems();
                
                // Show item review
                _itemReviewContainer.style.display = DisplayStyle.Flex;
                _itemReviewListView.style.display = DisplayStyle.Flex;
                _itemReviewListView.itemsSource = _viewModel.ImportItems;
                _itemReviewListView.Rebuild();
                
                // Show final import button
                _columnMappingContainer.style.display = DisplayStyle.None;
                _finalImportButton.style.display = DisplayStyle.Flex;
            }
            catch (Exception e)
            {
                ShowError($"Error applying mappings: {e.Message}");
            }
            
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

        private void ShowColumnMapping()
        {
            _root.Q<VisualElement>("item-review-container").style.display = DisplayStyle.None;
            _finalImportButton.style.display = DisplayStyle.None;
            _root.Q<VisualElement>("column-mapping-container").style.display = DisplayStyle.Flex;
            
            _processDataButton.text = "Next: Review Items";
            _processDataButton.clicked -= ShowItemReview;
            _processDataButton.clicked += ShowItemReview;
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