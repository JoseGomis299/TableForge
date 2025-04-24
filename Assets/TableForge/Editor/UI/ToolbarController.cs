using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ToolbarController
    {
        private readonly VisualElement _toolbar;
        private readonly TableVisualizer _tableVisualizer;

        private Button _addTabButton;
        private Button _transposeTableButton;
        
        public ToolbarController(VisualElement toolbar, TableVisualizer tableVisualizer)
        {
            _toolbar = toolbar;
            _tableVisualizer = tableVisualizer;
            
            Initialize();
        }

        private void Initialize()
        {
            BindVisualElements();
            RegisterEvents();
        }
        
        
        private void BindVisualElements()
        {
            _addTabButton = _toolbar.Q<Button>("add-tab-button");
            _transposeTableButton = _toolbar.Q<Button>("transpose-button");
        }

        private void RegisterEvents()
        {
            _addTabButton.RegisterCallback<ClickEvent>(e =>
            {
               
            });

            _transposeTableButton.RegisterCallback<ClickEvent>(e =>
            {
                _tableVisualizer.CurrentTable.Transpose();
                _tableVisualizer.CurrentTable.RebuildPage();
            });
        }


    }
}