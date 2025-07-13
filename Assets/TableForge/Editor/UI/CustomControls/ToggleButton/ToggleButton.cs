using System;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI.CustomControls
{
    [UxmlElement]
    public partial class ToggleButton : Button
    {
        public event Action<bool> OnValueChanged;
        public bool IsOn { get; private set; } = false;

        private const float OnOpacity = 1f;
        private const float OffOpacity = 0.35f;

        public ToggleButton() : base()
        {
            Init();
        }

        public ToggleButton(System.Action clickEvent) : base(clickEvent)
        {
            Init();
        }

        private void Init()
        {
            UpdateOpacity();

            clicked += () =>
            {
                SetState(!IsOn);
            };
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
            style.opacity = IsOn ? OnOpacity : OffOpacity;
        }
    }

}