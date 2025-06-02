using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class VisualElementResizer
    {
        private static readonly Dictionary<VisualElement, CheckSizeArguments> _checkSizeArguments = new();
        
        public static void ChangeSize(VisualElement element, float width, float height, Action onSuccess)
        {
            width = Mathf.Round(width);
            height = Mathf.Round(height);
            var targetSize = new Vector2(width, height);

            var initialWidth = Mathf.Round(element.resolvedStyle.width);
            var initialHeight = Mathf.Round(element.resolvedStyle.height);

            if (Mathf.Approximately(initialWidth, width) && 
                Mathf.Approximately(initialHeight, height))
            {
                onSuccess?.Invoke();
                return;
            }
            
            if (_checkSizeArguments.TryGetValue(element, out _))
            {
                element.UnregisterCallback<GeometryChangedEvent>(CheckSize);
                _checkSizeArguments.Remove(element);
            }
            
            _checkSizeArguments.Add(element, new CheckSizeArguments
            {
                Element = element,
                TargetSize = targetSize,
                OnSuccess = onSuccess
            });
            element.RegisterCallback<GeometryChangedEvent>(CheckSize);
            element.style.width = width;
            element.style.height = height;
        }
        
        private static void CheckSize(GeometryChangedEvent evt)
        {
            var element = evt.target as VisualElement;
            if (element == null) return;

            if (_checkSizeArguments.TryGetValue(element, out var args))
            {
                CheckSize(args.Element, args.TargetSize, args.OnSuccess);
            }
        }
        
        private static void CheckSize(VisualElement element, Vector2 targetSize, Action onSuccess)
        {
            var currentWidth = Mathf.Round(element.resolvedStyle.width);
            var currentHeight = Mathf.Round(element.resolvedStyle.height);
            var currentSize = new Vector2(currentWidth, currentHeight);

            if (currentSize == targetSize)
            {
                element.UnregisterCallback<GeometryChangedEvent>(CheckSize);
                onSuccess?.Invoke();
            }
        }
        
        private struct CheckSizeArguments
        {
            public VisualElement Element;
            public Vector2 TargetSize;
            public Action OnSuccess;
        }
    }
}