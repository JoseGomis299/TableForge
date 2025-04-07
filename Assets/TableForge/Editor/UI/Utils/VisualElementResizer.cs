using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class VisualElementResizer
    {
        private const int MaxAttempts = 3;
        private const long RetryDelay = 0;

        public static void ChangeSize(
            VisualElement element, 
            float width, 
            float height, 
            Action<GeometryChangedEvent> onSuccess,
            Action<string> onError = null)
        {
            width = Mathf.Round(width);
            height = Mathf.Round(height);

            var initialWidth = Mathf.Round(element.resolvedStyle.width);
            var initialHeight = Mathf.Round(element.resolvedStyle.height);

            if (Mathf.Approximately(initialWidth, width) && 
                Mathf.Approximately(initialHeight, height))
            {
                onSuccess?.Invoke(GeometryChangedEvent.GetPooled(element.worldBound, element.worldBound));
                return;
            }

            var targetSize = new Vector2(width, height);
            var attempts = 0;

            // Initial application
            element.style.width = width;
            element.style.height = height;
            element.schedule.Execute(() => CheckAndApplySize(element, width, height, targetSize, attempts, onSuccess, onError)).ExecuteLater(RetryDelay);
        }
        
        private static void CheckAndApplySize(VisualElement element, float width, float height, Vector2 targetSize, int attempts, Action<GeometryChangedEvent> onSuccess, Action<string> onError)
        {
            var currentWidth = Mathf.Round(element.resolvedStyle.width);
            var currentHeight = Mathf.Round(element.resolvedStyle.height);
            var currentSize = new Vector2(currentWidth, currentHeight);
            
            // Failure case
            if (attempts++ >= MaxAttempts)
            {
                onError?.Invoke($"Failed to resize after {MaxAttempts} attempts");
                return;
            }

            // Check if we need to apply width
            if (!Mathf.Approximately(currentWidth, width))
            {
                element.style.width = width;
                element.schedule.Execute(() => CheckAndApplySize(element, width, height, targetSize, attempts, onSuccess, onError)).ExecuteLater(RetryDelay);
                return;
            }

            // Check if we need to apply height
            if (!Mathf.Approximately(currentHeight, height))
            {
                element.style.height = height;
                element.schedule.Execute(() => CheckAndApplySize(element, width, height, targetSize, attempts, onSuccess, onError)).ExecuteLater(RetryDelay);
                return;
            }

            // Success case
            if (currentSize == targetSize)
            {
                onSuccess?.Invoke(GeometryChangedEvent.GetPooled(element.worldBound, element.worldBound));
            }
        }
        
    }
}