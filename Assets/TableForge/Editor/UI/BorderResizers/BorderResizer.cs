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
        public event Action<float> OnManualResize; 
        
        public bool IsResizing { get; private set; }
        protected abstract string ResizingPreviewClass { get; }
        
        protected readonly TableControl TableControl;
        
        //The headers that are currently being targeted for resizing
        protected readonly Dictionary<int, HeaderControl> ResizingHeaders = new Dictionary<int, HeaderControl>();
        
        //The header that is currently being resized
        protected HeaderControl ResizingHeader;
        
        private Vector3 _newSize;
        protected VisualElement ResizingPreview;

        
        protected BorderResizer(TableControl tableControl)
        {
            TableControl = tableControl;
            tableControl.Root.RegisterCallback<PointerMoveEvent>(CheckResize);
            tableControl.Root.RegisterCallback<PointerDownEvent>(StartResize, TrickleDown.TrickleDown);
            TableControl.Root.RegisterCallback<PointerDownEvent>(HandleDoubleClick, TrickleDown.TrickleDown);
        }

        protected abstract void CheckResize(PointerMoveEvent moveEvent);
        protected abstract void UpdateChildrenSize(HeaderControl headerControl);
        protected abstract float UpdateSize(HeaderControl headerControl, Vector3 newSize);
        protected abstract Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition);
        protected abstract void HandleDoubleClick(PointerDownEvent downEvent);
        protected abstract float InstantResize(HeaderControl target, bool adjustToStoredSize);
        protected abstract void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize);
        public abstract bool IsResizingArea(Vector3 position, out HeaderControl headerControl);

        public float ResizeCell(CellControl cellControl, bool storeSize = true)
        {
            float delta = 0;

            if (ResizingHeaders.TryGetValue(cellControl.Cell.Row.Id, out var header))
            {
                delta += InstantResize(header, false);
            }
            else if(ResizingHeaders.TryGetValue(cellControl.Cell.Column.Id, out header))
            {
                delta += InstantResize(header, false);
            }
         
            InvokeResize(header, delta, storeSize);
            return delta;
        }

        public float ResizeAll(bool adjustToStoredSize)
        {
            if(ResizingHeaders.Count == 0) return 0;

            float delta = 0;
            foreach (var header in ResizingHeaders.Values)
            {
                delta += InstantResize(header, adjustToStoredSize);
            }
            
            InvokeResize(ResizingHeaders.Values.First(x => x.Id != 0), delta, false);
            return delta;
        }

        public void HandleResize(HeaderControl target)
        {
            ResizingHeaders.Add(target.Id, target);
        }
        
        public void Dispose(HeaderControl target)
        {
            ResizingHeaders.Remove(target.Id);
        }
        
        protected void InvokeResize(HeaderControl target, float delta, bool storeSize)
        {
            if(delta == 0) return;
            
            target.RegisterSingleUseCallback<GeometryChangedEvent>(() =>
            {
                if (storeSize)
                {
                    int anchorId = target.CellAnchor?.Id ?? TableControl.Parent?.Cell.Id ?? 0;
                    TableControl.Metadata.SetAnchorSize(anchorId,
                        new Vector2(target.resolvedStyle.width, target.resolvedStyle.height));
                }

                OnResize?.Invoke(delta);
            });
        }
        
        protected void InvokeManualResize(HeaderControl target, float delta)
        {
            if(delta == 0) return;
            
            target.RegisterSingleUseCallback<GeometryChangedEvent>(() =>
            {
                OnManualResize?.Invoke(delta);
            });
        }
        
        private void StartResize(PointerDownEvent downEvent)
        {
            if (ResizingHeader == null || downEvent.button != 0 || TableControl is { enabledInHierarchy: false }) return;
            
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
                InvokeResize(ResizingHeader, delta, true);
                InvokeManualResize(ResizingHeader, delta);
                ResizingPreview.RemoveFromHierarchy();
                
                TableControl.Root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                TableControl.Root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                IsResizing = false;
            }
        }
    }
}
