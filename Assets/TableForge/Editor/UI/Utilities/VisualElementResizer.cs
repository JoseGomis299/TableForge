using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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
                _checkSizeArguments.Remove(element);
            }
            
            _checkSizeArguments.Add(element, new CheckSizeArguments
            {
                element = element,
                targetSize = targetSize,
                onSuccess = onSuccess
            });
            
            element.schedule.Execute(() =>
            {
                var args = _checkSizeArguments[element];
                args.element.style.width = args.targetSize.x;
                args.element.style.height = args.targetSize.y;
                args.element.RegisterCallback<GeometryChangedEvent>(CheckSize);
                args.element.schedule.Execute(args.onSuccess).ExecuteLater(0);
            }).ExecuteLater(0);
        }
        
        private static void CheckSize(GeometryChangedEvent evt)
        {
            var element = evt.target as VisualElement;
            if (element == null) return;

            if (_checkSizeArguments.TryGetValue(element, out var args))
            {
                CheckSize(args.element, args.targetSize, args.onSuccess);
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
            public VisualElement element;
            public Vector2 targetSize;
            public Action onSuccess;
        }
    }
}