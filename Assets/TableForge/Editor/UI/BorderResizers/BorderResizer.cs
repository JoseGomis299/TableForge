using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class BorderResizer
    {
        public event Action<float> OnResize; 
        
        public bool IsResizing { get; private set; }
        protected abstract string ResizingPreviewClass { get; }
        
        protected readonly TableControl TableControl;
        
        //The headers that are currently being targeted for resizing
        protected readonly HashSet<HeaderControl> ResizingHeaders = new HashSet<HeaderControl>();
        
        //The header that is currently being resized
        protected HeaderControl ResizingHeader;
        
        private Vector3 _newSize;
        protected VisualElement ResizingPreview;

        
        protected BorderResizer(TableControl tableControl)
        {
            TableControl = tableControl;
            tableControl.Root.RegisterCallback<PointerMoveEvent>(CheckResize);
            tableControl.Root.RegisterCallback<PointerDownEvent>(StartResize);
            TableControl.Root.RegisterCallback<PointerDownEvent>(HandleDoubleClick);
        }

        protected abstract void CheckResize(PointerMoveEvent moveEvent);
        protected abstract void UpdateChildrenSize(HeaderControl headerControl);
        protected abstract float UpdateSize(HeaderControl headerControl, Vector3 newSize);
        protected abstract Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition);
        protected abstract void HandleDoubleClick(PointerDownEvent downEvent);
        protected abstract float InstantResize(HeaderControl target);
        protected abstract void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize);
        public abstract bool IsResizingArea(Vector3 position, out HeaderControl headerControl);


        public float ResizeAll()
        {
            if(ResizingHeaders.Count == 0) return 0;

            float delta = 0;
            foreach (var header in ResizingHeaders)
            {
                delta += InstantResize(header);
            }
            
            InvokeResize(ResizingHeaders.First(), delta);
            return delta;
        }

        public void HandleResize(HeaderControl target)
        {
            ResizingHeaders.Add(target);
        }
        
        public void Dispose(HeaderControl target)
        {
            ResizingHeaders.Remove(target);
        }
        
        protected void InvokeResize(HeaderControl target, float delta)
        {
            target.RegisterSingleUseCallback<GeometryChangedEvent>(() => OnResize?.Invoke(delta));
        }
        
        private void StartResize(PointerDownEvent downEvent)
        {
            if (ResizingHeader == null || downEvent.button != 0) return;
            
            IsResizing = true;
            var initialSize = new Vector2(ResizingHeader.resolvedStyle.width, ResizingHeader.resolvedStyle.height);
            var startPosition = downEvent.position;
            _newSize = initialSize;

            ResizingPreview = new VisualElement();
            ResizingPreview.AddToClassList(ResizingPreviewClass);
            TableControl.Add(ResizingPreview);
            MovePreview(startPosition, initialSize, initialSize);

            TableControl.Root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            TableControl.Root.RegisterCallback<PointerUpEvent>(OnPointerUp);
            downEvent.StopImmediatePropagation();

            void OnPointerMove(PointerMoveEvent moveEvent)
            {
                if (!IsResizing || moveEvent.pressedButtons != 1)
                {
                    UnregisterCallbacks();
                    return;
                }
                
                _newSize = CalculateNewSize(initialSize, startPosition, moveEvent.position);
                MovePreview(startPosition, initialSize, _newSize);
            }
            
            void OnPointerUp(PointerUpEvent upEvent)
            {
                UnregisterCallbacks();
            }

            void UnregisterCallbacks()
            {
                float delta = UpdateSize(ResizingHeader, _newSize);
                UpdateChildrenSize(ResizingHeader);
                InvokeResize(ResizingHeader, delta);
                ResizingPreview.RemoveFromHierarchy();

                TableControl.Root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                TableControl.Root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                IsResizing = false;
            }
        }
    }
}
