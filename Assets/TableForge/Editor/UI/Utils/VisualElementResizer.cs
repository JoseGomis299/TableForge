using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class VisualElementResizer
    {
        public static void ChangeSize(VisualElement element, float width, float height, Action<GeometryChangedEvent> onSuccess)
        {
            width = Mathf.Round(width);
            height = Mathf.Round(height);
            var targetSize = new Vector2(width, height);

            var initialWidth = Mathf.Round(element.resolvedStyle.width);
            var initialHeight = Mathf.Round(element.resolvedStyle.height);

            if (Mathf.Approximately(initialWidth, width) && 
                Mathf.Approximately(initialHeight, height))
            {
                onSuccess?.Invoke(GeometryChangedEvent.GetPooled(element.worldBound, element.worldBound));
                return;
            }
            
            element.UnregisterCallback<GeometryChangedEvent>(_ => CheckSize(element, targetSize, onSuccess));
            element.RegisterCallback<GeometryChangedEvent>(_ => CheckSize(element, targetSize, onSuccess));
            element.style.width = width;
            element.style.height = height;
            
            CheckSize(element, targetSize, onSuccess);
        }
        
        private static void CheckSize(VisualElement element, Vector2 targetSize, Action<GeometryChangedEvent> onSuccess)
        {
            var currentWidth = Mathf.Round(element.resolvedStyle.width);
            var currentHeight = Mathf.Round(element.resolvedStyle.height);
            var currentSize = new Vector2(currentWidth, currentHeight);

            if (currentSize == targetSize)
            {
                element.UnregisterCallback<GeometryChangedEvent>(_ => CheckSize(element, targetSize, onSuccess));
                onSuccess?.Invoke(GeometryChangedEvent.GetPooled(element.worldBound, element.worldBound));
            }
        }
    }
}