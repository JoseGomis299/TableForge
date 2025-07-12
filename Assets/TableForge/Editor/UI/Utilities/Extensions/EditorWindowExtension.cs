
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    public static class EditorWindowExtension
    {
        private static readonly Dictionary<EditorWindow, VisualElement> _activeModals = new();

        public static void ShowModal(this EditorWindow window, VisualElement content, int width = 400, Color? backgroundColor = null)
        {
            // Close any existing modal first
            window.CloseModal();

            var root = window.rootVisualElement;
            
            // Create overlay
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.right = 0;
            overlay.style.top = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            overlay.pickingMode = PickingMode.Position;

            // Create modal container
            var modalContainer = new VisualElement();
            modalContainer.style.width = width;
            modalContainer.style.backgroundColor = backgroundColor ?? new Color(0.2f, 0.2f, 0.2f);
            modalContainer.style.marginLeft = modalContainer.style.marginTop = Length.Auto();
            modalContainer.style.left = modalContainer.style.top = new Length(50, LengthUnit.Percent);
            modalContainer.style.translate = new Translate(-50, -50);
            modalContainer.pickingMode = PickingMode.Position;

            // Add content to modal container
            modalContainer.Add(content);

            // Handle click outside modal
            overlay.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == overlay)
                    window.CloseModal();
            });

            // Handle Escape key
            modalContainer.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                    window.CloseModal();
            });

            overlay.Add(modalContainer);
            root.Add(overlay);
            _activeModals[window] = overlay;
        }

        public static void CloseModal(this EditorWindow window)
        {
            if (_activeModals.TryGetValue(window, out var modal))
            {
                modal.RemoveFromHierarchy();
                _activeModals.Remove(window);
            }
        }
    }
}