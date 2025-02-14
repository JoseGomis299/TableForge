using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableVisualizer : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("TableForge/TableVisualizer")]
        public static void ShowExample() => GetWindow<TableVisualizer>("TableVisualizer");

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(visualTreeAsset.Instantiate());

            UiConstants.InitializeStyles(root);
            UiConstants.OnStylesInitialized += OnStylesInitialized;
        }

        private void OnStylesInitialized()
        {
            var root = rootVisualElement;
            var mainTable = root.Q<VisualElement>("MainTable");
            var table = TableManager.GenerateTables()[1];

            var tableControl = new TableControl(rootVisualElement);
            tableControl.SetTable(table);
            mainTable.Add(tableControl);

            UiConstants.OnStylesInitialized -= OnStylesInitialized;
        }
    }
}