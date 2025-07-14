using UnityEngine.UIElements;

namespace TableForge.Editor.UI.CustomControls
{
    internal abstract class ButtonBehaviorOverrider
    {
        public Button Button { get; }

        protected ButtonBehaviorOverrider(Button button)
        {
            Button = button;
            button.clicked += OnButtonClicked;
        }
        
        protected abstract void OnButtonClicked();
    }
}