using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using TableForge.Editor.Serialization;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace TableForge.Editor.UI
{
    public class ExportTableWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        
        private const int MaxPreviewRows = 10;
        private Table _table;
        
        //UI Elements
        private ObjectField _objectField;
        private EnumField _formatDropdown;
        private Toggle _includeGuidsToggle;
        private Toggle _includePathsToggle;
        private Toggle _flattenToggle;
        private TextField _previewField;
        private Button _exportButton;
        
        [MenuItem("Window/TableForge/Export Table")]
        public static void ShowWindow()
        {
            ExportTableWindow w = GetWindow<ExportTableWindow>("Table Exporter");
            w.minSize = new Vector2(490, 470);
        }

        public void CreateGUI()
        {
            rootVisualElement.Add(visualTreeAsset.Instantiate());

            // Query elements
            _objectField = rootVisualElement.Q<ObjectField>("metadata-object-field");
            _formatDropdown = rootVisualElement.Q<EnumField>("serialization-format-dropdown");
            _includeGuidsToggle = rootVisualElement.Q<Toggle>("include-guids-toggle");
            _includePathsToggle = rootVisualElement.Q<Toggle>("include-paths-toggle");
            _flattenToggle = rootVisualElement.Q<Toggle>("flatten-subtables-toggle");
            _previewField = rootVisualElement.Q<TextField>("preview-text-field");
            _exportButton = rootVisualElement.Q<Button>("export-button");

            // Setup format dropdown
            _formatDropdown.Init(SerializationFormat.Csv);
            _objectField.objectType = typeof(TableMetadata);
            
            // Register callbacks
            _objectField.RegisterValueChangedCallback(OnInputChanged);
            _formatDropdown.RegisterValueChangedCallback(OnFormatChanged);
            _includeGuidsToggle.RegisterValueChangedCallback(OnInputChanged);
            _includePathsToggle.RegisterValueChangedCallback(OnInputChanged);
            _flattenToggle.RegisterValueChangedCallback(OnInputChanged);
            _exportButton.clicked += OnExportClicked;

            // Initial update
            UpdatePreview();
            UpdateFlattenToggleVisibility();
        }

        private void OnFormatChanged(ChangeEvent<Enum> evt)
        {
            UpdateFlattenToggleVisibility();
            UpdatePreview();
        }

        private void UpdateFlattenToggleVisibility()
        {
            _flattenToggle.visible = _formatDropdown.value is SerializationFormat.Csv;
        }

        private void OnInputChanged(ChangeEvent<Object> _) => UpdatePreview();
        private void OnInputChanged(ChangeEvent<bool> _) => UpdatePreview();

        private void UpdatePreview()
        {
            try
            {
                var table = GetSelectedTable();
                if (table == null) return;

                var args = CreateSerializationArgs(table);
                string previewText = TableSerializer.SerializeTable(args, MaxPreviewRows);
                if(table.Rows.Count > 10) previewText = $"Preview limited to first {MaxPreviewRows} rows:\n{previewText}";
                _previewField.value = previewText;
            }
            catch
            {
                _previewField.value = "Invalid table configuration";
            }
        }

        private Table GetSelectedTable()
        {
            if (_objectField.value is TableMetadata metadata)
            {
                if(_table != null && metadata.Name == _table.Name)
                    return _table; // Return cached table if it's already loaded
                
                _table = TableMetadataManager.GetTable(metadata);
                return _table;
            }
            return null;
        }

        private TableSerializationArgs CreateSerializationArgs(Table table)
        {
            var format = (SerializationFormat) _formatDropdown.value;

            if (format == SerializationFormat.Csv)
            {
                return new CsvTableSerializationArgs(
                    table,
                    _includeGuidsToggle.value,
                    _includePathsToggle.value,
                    _flattenToggle.value
                );
            }
            
            return new JsonTableSerializationArgs(
                table,
                _includeGuidsToggle.value,
                _includePathsToggle.value
            );
        }

        private void OnExportClicked()
        {
            var table = GetSelectedTable();
            if (table == null) return;

            var extension = _formatDropdown.value.ToString().ToLower();
            var path = EditorUtility.SaveFilePanel(
                "Export Table",
                "",
                $"{table.Name}.{extension}",
                extension
            );

            if (!string.IsNullOrEmpty(path))
            {
                var args = CreateSerializationArgs(table);
                File.WriteAllText(path, TableSerializer.SerializeTable(args));
                AssetDatabase.Refresh();
                
                Close();
                EditorUtility.DisplayDialog("Export Successful", $"Table '{table.Name}' exported successfully to:\n{path}", "OK");
            }
        }
    }
}