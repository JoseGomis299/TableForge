using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class BorderResizer
    {
        protected readonly TableControl TableControl;
        
        //The headers that are currently being targeted for resizing
        protected readonly HashSet<HeaderControl> ResizingHeaders = new HashSet<HeaderControl>();
        
        //The header that is currently being resized
        protected HeaderControl ResizingHeader;
        
        protected bool IsResizing;
        
        protected BorderResizer(TableControl tableControl)
        {
            TableControl = tableControl;
            tableControl.Root.RegisterCallback<PointerMoveEvent>(CheckResize);
            tableControl.Root.RegisterCallback<PointerDownEvent>(StartResize);
            TableControl.Root.RegisterCallback<PointerDownEvent>(HandleDoubleClick);

        }

        protected abstract void CheckResize(PointerMoveEvent moveEvent);
        protected abstract void UpdateChildrenSize(HeaderControl headerControl, float newWidth);
        protected abstract float CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition);
        protected abstract void HandleDoubleClick(PointerDownEvent downEvent);
        protected abstract void InstantResize(HeaderControl target);
        public abstract bool IsResizingArea(Vector3 position, out HeaderControl headerControl);


        public void ResizeAll()
        {
            foreach (var target in ResizingHeaders)
            {
               InstantResize(target);
            }
        }

        public void HandleResize(HeaderControl target)
        {
            ResizingHeaders.Add(target);
        }
        
        public void Dispose(HeaderControl target)
        {
            ResizingHeaders.Remove(target);
        }
        
        private void StartResize(PointerDownEvent downEvent)
        {
            if (ResizingHeader == null || downEvent.button != 0) return;
            
            IsResizing = true;
            var initialSize = new Vector2(ResizingHeader.resolvedStyle.width, ResizingHeader.resolvedStyle.height);
            var startPosition = downEvent.position;

            TableControl.Root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            downEvent.StopPropagation();

            void OnPointerMove(PointerMoveEvent moveEvent)
            {
                if (!IsResizing || moveEvent.pressedButtons != 1)
                {
                    UnregisterCallbacks();
                    return;
                }
                
                float newWidth = CalculateNewSize(initialSize, startPosition, moveEvent.position);
                UpdateChildrenSize(ResizingHeader, newWidth);
            }

            void UnregisterCallbacks()
            {
                TableControl.Root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                IsResizing = false;
            }
        }
    }
}
