using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class TableVisualizer : EditorWindow
    {
        private double _lastUpdateTime;
        private TableControl _tableControl;
        private ToolbarController _toolbarController;
        
        public TableControl CurrentTable => _tableControl;
        public ToolbarController ToolbarController => _toolbarController;

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("Window/TableForge/Table Visualizer", priority = 0)]
        public static void Initialize() => GetWindow<TableVisualizer>("Table Visualizer");
        
        private void CreateGUI()
        {
            rootVisualElement.focusable = true;
            rootVisualElement.Add(visualTreeAsset.Instantiate());
            
            UiConstants.OnStylesInitialized += PopulateWindow;
            UiConstants.InitializeStyles(rootVisualElement[0]);
        }

        private void PopulateWindow()
        {
            var mainTable = rootVisualElement.Q<VisualElement>("MainTable");
            
            var tableAttributes = new TableAttributes
            {
                tableType = TableType.Dynamic,
                columnReorderMode = TableReorderMode.ExplicitReorder,
                rowReorderMode = TableReorderMode.ExplicitReorder,
                columnHeaderVisibility = TableSettings.GetSettings().columnHeaderVisibility,
                rowHeaderVisibility = TableSettings.GetSettings().rowHeaderVisibility,
            };
            
            _tableControl = new TableControl(rootVisualElement, tableAttributes, null, null, this);
            mainTable.Add(_tableControl);

            var toolbar = rootVisualElement.Q<VisualElement>("toolbar");
            _toolbarController = new ToolbarController(toolbar, this);
            
            UiConstants.OnStylesInitialized -= PopulateWindow;
            
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update += Update;
            InspectorChangeNorifier.OnScriptableObjectModified += OnScriptableObjectModified;
        }

        public void SetTable(Table table)
        {
            if(_tableControl == null) return;
            
            _tableControl.CellSelector.ClearSelection();
            _tableControl.SetTable(table);
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
                    .Where(row => !PathUtil.TryLoadAsset(AssetDatabase.GetAssetPath(row.SerializedObject.RootObject), out _))
                    .ToList();
                
                foreach (var row in missingRows)
                {
                    UndoRedoManager.RemoveRelatedCommandsFromStack(row.SerializedObject.RootObjectGuid);
                    metadata.RemoveItemGuid(row.SerializedObject.RootObjectGuid);
                    _tableControl.RemoveRow(row.Id);
                }
                
                _tableControl.RebuildPage();
                return;
            }
            
            _tableControl.UpdateAll();
        }
        
        private void OnScriptableObjectModified(ScriptableObject scriptableObject)
        {
            if(_tableControl == null) return;
            
            Row row = _tableControl.TableData.Rows.Values.FirstOrDefault(r => r.SerializedObject.RootObject == scriptableObject);
            if(row == null) return;
            
            _tableControl.UpdateRow(row.Id);
        }

        private void Update()
        {
            if(_tableControl == null) return;
            if(!TableSettings.GetSettings().enablePolling || _lastUpdateTime >= EditorApplication.timeSinceStartup - TableSettings.GetSettings().pollingInterval)
                return;
        
            _lastUpdateTime = EditorApplication.timeSinceStartup;
            _tableControl?.Update();
        }
    }
}