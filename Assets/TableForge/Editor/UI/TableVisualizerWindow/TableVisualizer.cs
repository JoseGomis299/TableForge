using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace TableForge.UI
{
    internal class TableVisualizer : EditorWindow
    {
        private double _lastUpdateTime;
        private TableControl _tableControl;
        private ToolbarController _toolbarController;
        
        public TableControl CurrentTable => _tableControl;
        
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("TableForge/TableVisualizer")]
        public static void ShowExample() => GetWindow<TableVisualizer>("TableVisualizer");

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.focusable = true;
            root.Add(visualTreeAsset.Instantiate());

            UiConstants.InitializeStyles(root[0]);
            UiConstants.OnStylesInitialized += OnStylesInitialized;
        }

        private void OnStylesInitialized()
        {
            Stopwatch sw = new Stopwatch();

            var root = rootVisualElement;
            var mainTable = root.Q<VisualElement>("MainTable");
            sw.Start();
            var table = TableManager.GenerateTables()[1];

                        
            var tableAttributes = new TableAttributes()
            {
                TableType = TableType.Dynamic,
                ColumnReorderMode = TableReorderMode.ExplicitReorder,
                RowReorderMode = TableReorderMode.ExplicitReorder,
                ColumnHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
                RowHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
            };
            sw.Stop();
            float timeToGenerate = sw.ElapsedMilliseconds;
            sw.Reset();
            sw.Start();

            var toolbar = root.Q<VisualElement>("toolbar");
            _toolbarController = new ToolbarController(toolbar, this);

            _tableControl = new TableControl(rootVisualElement, tableAttributes, null);
            _tableControl.SetTable(table);

            mainTable.schedule.Execute(() =>
            {
                mainTable.Add(_tableControl);
                
                EditorApplication.update += Update;
                InspectorChangeNorifier.OnScriptableObjectModified += OnScriptableObjectModified;
                UiConstants.OnStylesInitialized -= OnStylesInitialized;
                
                sw.Stop();
                Debug.Log($"Time to generate table: {timeToGenerate}ms");
                Debug.Log($"Time to initialize table: {sw.ElapsedMilliseconds}ms");
                Debug.Log($"Total time: {timeToGenerate + sw.ElapsedMilliseconds}ms");
            }).ExecuteLater(0);
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