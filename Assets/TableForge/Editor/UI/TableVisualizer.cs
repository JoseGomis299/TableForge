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

            UiContants.InitializeStyles(root);
            UiContants.OnStylesInitialized += OnStylesInitialized;
        }

        private void OnStylesInitialized()
        {
            var root = rootVisualElement;
            var scrollView = root.Q<ScrollView>("ScrollView");
            var table = TableManager.GenerateTables()[1];

            var tableControl = new TableControl(rootVisualElement);
            tableControl.SetTable(table);
            scrollView.Add(tableControl);

            UiContants.OnStylesInitialized -= OnStylesInitialized;
        }
    }
}