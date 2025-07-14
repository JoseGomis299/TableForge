using System;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI.CustomControls
{
    internal class ToggleButton : ButtonBehaviorOverrider
    {
        public event Action<bool> OnValueChanged;
        public bool IsOn { get; private set; } = false;

        private const float OnOpacity = 1f;
        private const float OffOpacity = 0.35f;

        public ToggleButton(Button button) : base(button)
        {
            UpdateOpacity();
        }

        protected override void OnButtonClicked()
        {
            SetState(!IsOn);
        }

        public void SetState(bool state)
        {
            if (IsOn != state)
            {
                IsOn = state;
                UpdateOpacity();
                
                OnValueChanged?.Invoke(IsOn);
            }
        }

        private void UpdateOpacity()
        {
            Button.style.opacity = IsOn ? OnOpacity : OffOpacity;
        }
    }

}