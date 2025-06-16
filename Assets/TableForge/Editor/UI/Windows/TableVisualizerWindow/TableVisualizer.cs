using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableVisualizer : EditorWindow
    {
        private double _lastUpdateTime;
        private TableControl _tableControl;
        private ToolbarController _toolbarController;
        
        public TableControl CurrentTable => _tableControl;
        public ToolbarController ToolbarController => _toolbarController;

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("TableForge/TableVisualizer")]
        public static void Initialize() => GetWindow<TableVisualizer>("TableVisualizer");
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;
            root.Add(visualTreeAsset.Instantiate());

            UiConstants.InitializeStyles(root[0]);
            UiConstants.OnStylesInitialized += OnStylesInitialized;
        }
        
        public void SetTable(Table table)
        {
            if(_tableControl == null) return;
            
            _tableControl.CellSelector.ClearSelection();
            _tableControl.SetTable(table);
        }

        private void OnStylesInitialized()
        {
            var root = rootVisualElement;
            var mainTable = root.Q<VisualElement>("MainTable");
            
            var tableAttributes = new TableAttributes()
            {
                TableType = TableType.Dynamic,
                ColumnReorderMode = TableReorderMode.ExplicitReorder,
                RowReorderMode = TableReorderMode.ExplicitReorder,
                ColumnHeaderVisibility = TableHeaderVisibility.ShowHeaderLetterAndName,
                RowHeaderVisibility = TableHeaderVisibility.ShowHeaderNumberAndName,
            };

            _tableControl = new TableControl(rootVisualElement, tableAttributes, null, null, this);
            mainTable.Add(_tableControl);
            
            var toolbar = root.Q<VisualElement>("toolbar");
            _toolbarController = new ToolbarController(toolbar, this);

            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update += Update;
            InspectorChangeNorifier.OnScriptableObjectModified += OnScriptableObjectModified;
            UiConstants.OnStylesInitialized -= OnStylesInitialized;
        }
        
        private void OnProjectChanged()
        {
            if(_tableControl == null || _toolbarController.SelectedTab == null) return;
            
            TableMetadata metadata = _toolbarController.SelectedTab;
            metadata.UpdateRowsPosition();
            
            // If the number of items in the table has changed, we need to create a new table with the new items.
            if (metadata.IsTypeBound &&
                (_tableControl.TableData == null || metadata.ItemGUIDs.Count != _tableControl.TableData.Rows.Count))
            {
                Table table = TableMetadataManager.GetTable(metadata);
                _toolbarController.UpdateTableCache(metadata, table);
                _tableControl.SetTable(table);
                return;
            }
            
            // If the table is not type bound, we need to check if any tracked items have been removed.
            if (!metadata.IsTypeBound && _tableControl.TableData != null)
            {
                var missingRows = _tableControl.TableData.Rows.Values
                    .Where(row => !AssetDatabase.AssetPathExists(AssetDatabase.GetAssetPath(row.SerializedObject.RootObject)))
                    .ToList();
                
                foreach (var row in missingRows)
                {
                    metadata.RemoveItemGUID(row.SerializedObject.RootObjectGuid);
                    _tableControl.RemoveRow(row.Id);
                }
                
                _tableControl.RebuildPage();
                return;
            }
            
            _tableControl.UpdateAll();
        }
        
        private void OnScriptableObjectModified(ScriptableObject scriptableObject)
        {
            Row row = _tableControl.TableData.Rows.Values.FirstOrDefault(r => r.SerializedObject.RootObject == scriptableObject);
            if(row == null) return;
            
            _tableControl.UpdateRow(row.Id);
        }

        private void Update()
        {
            if(!ToolbarData.EnablePolling || _lastUpdateTime >= EditorApplication.timeSinceStartup - ToolbarData.RefreshRate)
                return;
        
            _lastUpdateTime = EditorApplication.timeSinceStartup;
            _tableControl?.Update();
        }
    }
}