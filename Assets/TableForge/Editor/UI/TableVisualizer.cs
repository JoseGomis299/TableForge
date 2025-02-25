using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableVisualizer : EditorWindow
    {
        private double _lastUpdateTime;
        private TableControl _tableControl;
        
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("TableForge/TableVisualizer")]
        public static void ShowExample() => GetWindow<TableVisualizer>("TableVisualizer");

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(visualTreeAsset.Instantiate());

            UiConstants.InitializeStyles(root[0]);
            UiConstants.OnStylesInitialized += OnStylesInitialized;
        }

        private void OnStylesInitialized()
        {
            var root = rootVisualElement;
            var mainTable = root.Q<VisualElement>("MainTable");
            var table = TableManager.GenerateTables()[1];

                        
            var tableAttributes = new TableAttributes()
            {
                TableType = TableType.Dynamic,
                ColumnReorderMode = TableReorderMode.ExplicitReorder,
                RowReorderMode = TableReorderMode.ExplicitReorder,
                ColumnHeaderVisibility = TableHeaderVisibility.ShowHeaderLetterAndName,
                RowHeaderVisibility = TableHeaderVisibility.ShowHeaderNumberAndName,
            };
            
            _tableControl = new TableControl(rootVisualElement, tableAttributes, null);
            _tableControl.SetTable(table);
            mainTable.Add(_tableControl);
            
            EditorApplication.update += Update;
            UiConstants.OnStylesInitialized -= OnStylesInitialized;
        }

        private void Update()
        {
            if(_lastUpdateTime >= EditorApplication.timeSinceStartup - ToolbarData.RefreshRate)
                return;

            _lastUpdateTime = EditorApplication.timeSinceStartup;
            _tableControl?.Update();
        }
    }
}