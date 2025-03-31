using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class VisualElementResizer
    {
        private static HashSet<VisualElement> _resizingElements = new();
        private static Dictionary<VisualElement, Queue<Action>> _resizeQueue = new();
        
        public static void ChangeSize(VisualElement element, float width, float height, Action<SizeChangedEvent> callback)
        {
            if (_resizingElements.Contains(element))
            {
                Debug.Log(element + " is resizing");
                if (!_resizeQueue.ContainsKey(element))
                    _resizeQueue.Add(element, new Queue<Action>());
                
                _resizeQueue[element].Enqueue(() => ChangeSize(element, width, height, callback));
                return;
            }

            var prevSize = new Vector2(element.style.width.value.value, element.style.height.value.value);
            var newSize = new Vector2(width, height);
            SizeChangedEvent sizeChangedEvent = new SizeChangedEvent(element, prevSize, newSize);
            
            if (!Mathf.Approximately(prevSize.x, newSize.x))
            {
                _resizingElements.Add(element);

                element.style.width = width;
                element.RegisterSingleUseCallback<GeometryChangedEvent>(() =>
                {
                    if (!Mathf.Approximately(prevSize.y, newSize.y))
                    {
                        element.style.height = height;
                        element.RegisterSingleUseCallback<GeometryChangedEvent>(() => OnSizeChangeCompleted(sizeChangedEvent, callback));
                    }
                    else OnSizeChangeCompleted(sizeChangedEvent, callback);
                });
            }
            else if (!Mathf.Approximately(prevSize.y, newSize.y))
            {
                _resizingElements.Add(element);

                element.style.height = height;
                element.RegisterSingleUseCallback<GeometryChangedEvent>(() => OnSizeChangeCompleted(sizeChangedEvent, callback));
            }
        }

        private static void OnSizeChangeCompleted(SizeChangedEvent evt, Action<SizeChangedEvent> callback)
        {
            _resizingElements.Remove(evt.Target);
            callback?.Invoke(evt);

            if (_resizeQueue.ContainsKey(evt.Target) && _resizeQueue[evt.Target].Count > 0)
            {
                _resizeQueue[evt.Target].Dequeue()?.Invoke();
            }
        }
    }
    
    
    internal readonly struct SizeChangedEvent
    {
        public VisualElement Target { get; }
        public Vector2 PrevSize { get; }
        public Vector2 NewSize { get; }

        public SizeChangedEvent(VisualElement target, Vector2 prevSize, Vector2 newSize)
        {
            Target = target;
            PrevSize = prevSize;
            NewSize = newSize;
        }
    }
}