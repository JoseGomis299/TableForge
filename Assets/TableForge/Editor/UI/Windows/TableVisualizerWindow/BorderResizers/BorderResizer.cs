using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class BorderResizer
    {
        public event Action<float> OnResize; 
        public event Action<float> OnManualResize;

        public bool IsResizing
        {
            get => _isResizing;
            private set
            {
                _isResizing = value;
                tableControl.CellSelector.SelectionEnabled = !value;
            }
        }
        protected abstract string ResizingPreviewClass { get; }
        
        protected readonly TableControl tableControl;
        
        //The headers that are currently being targeted for resizing
        protected readonly Dictionary<int, HeaderControl> resizingHeaders = new();
        protected readonly HashSet<int> excludedFromManualResizing = new();
        
        //The header that is currently being resized
        protected HeaderControl resizingHeader;
        
        protected VisualElement resizingPreview;
        
        private Vector3 _newSize;
        private bool _isResizing;
    
        
        protected BorderResizer(TableControl tableControl)
        {
            this.tableControl = tableControl;
            tableControl.Root.RegisterCallback<PointerMoveEvent>(CheckResize);
            tableControl.Root.RegisterCallback<PointerDownEvent>(StartResize, TrickleDown.TrickleDown);
            this.tableControl.Root.RegisterCallback<PointerDownEvent>(HandleDoubleClick, TrickleDown.TrickleDown);
        }

        protected abstract void CheckResize(PointerMoveEvent moveEvent);
        protected abstract void UpdateChildrenSize(HeaderControl headerControl);
        protected abstract float UpdateSize(HeaderControl headerControl, Vector3 newSize);
        protected abstract Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition);
        protected abstract void HandleDoubleClick(PointerDownEvent downEvent);
        protected abstract float InstantResize(HeaderControl target, bool fitStoredSize);
        protected abstract void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize);
        public abstract bool IsResizingArea(Vector3 position, out HeaderControl headerControl);

        public float ResizeCell(CellControl cellControl, bool storeSize = true)
        {
            float delta = 0;

            if (resizingHeaders.TryGetValue(cellControl.Cell.row.Id, out var header))
            {
                delta += InstantResize(header, false);
            }
            else if(resizingHeaders.TryGetValue(cellControl.Cell.column.Id, out header))
            {
                delta += InstantResize(header, false);
            }
         
            InvokeResize(new List<HeaderControl> {header}, delta, storeSize, false, Vector2.zero);
            return delta;
        }

        public float ResizeAll(bool fitStoredSize, bool storeSize)
        {
            if(resizingHeaders.Count == 0) return 0;
            
            float delta = 0;
            foreach (var header in resizingHeaders.Values)
            {
                delta += InstantResize(header, fitStoredSize);
            }

            InvokeResize(resizingHeaders.Values.Where(x => x.Id != 0).ToList(), delta, storeSize, fitStoredSize, Vector2.zero, false);
            return delta;
        }
        
        public float ResizeHeader(HeaderControl target, bool storeSize = true, bool fitStoredSize = false)
        {
            if (target == null) return 0;

            float delta = InstantResize(target, fitStoredSize);
            InvokeResize(new List<HeaderControl>{target}, delta, storeSize, fitStoredSize, Vector2.zero);
            return delta;
        }

        public void HandleResize(HeaderControl target, bool excludeFromManualResizing = false)
        {
            resizingHeaders.Add(target.Id, target);
            
            if(excludeFromManualResizing) 
                excludedFromManualResizing.Add(target.Id);
        }
        
        public void Dispose(HeaderControl target)
        {
            resizingHeaders.Remove(target.Id);
            excludedFromManualResizing.Remove(target.Id);
        }
        
        public void Clear()
        {
            resizingHeaders.Clear();
            excludedFromManualResizing.Clear();
        }
        
        protected void InvokeResize(List<HeaderControl> targets, float delta, bool storeSize, bool fitStoredSize, Vector2 targetSize, bool extendToAncestors = true)
        {
            if(delta == 0 || targets == null || targets.Count == 0 || targets[0] == null) return;
            var target = targets[0];

            if (targetSize == Vector2.zero)
            {
                targetSize = fitStoredSize
                    ? tableControl.Metadata.GetAnchorSize(target.CellAnchor.Id)
                    : tableControl.PreferredSize.GetHeaderSize(target.CellAnchor);
            }

            var sizeIsSet = target is RowHeaderControl 
                ? Mathf.Approximately(Mathf.Round(target.resolvedStyle.height), Mathf.Round(targetSize.y)) 
                : Mathf.Approximately(Mathf.Round(target.resolvedStyle.width), Mathf.Round(targetSize.x));

            if(sizeIsSet)
            {
                Invoke(extendToAncestors);
            }
            else
            {
                target.RegisterSingleUseCallback<GeometryChangedEvent>(_ =>
                {
                    Invoke(extendToAncestors);
                });
            }
            
            
            void Invoke(bool extendToAncestors)
            {
                if (storeSize){

                    foreach (var target in targets)
                    {
                        int anchorId = target.CellAnchor?.Id ?? tableControl.Parent?.Cell.Id ?? 0;
                        float width = target.resolvedStyle.width;
                        float height = target.resolvedStyle.height;
                        Vector2 sizeToStore = tableControl.Metadata.GetAnchorSize(anchorId);
                    
                        bool isRow = target is RowHeaderControl;

                        if (isRow)
                        {
                            sizeToStore.y = height;
                        }
                        else
                        {
                            sizeToStore.x = width;
                        }
                    
                        tableControl.Metadata.SetAnchorSize(anchorId, sizeToStore);
                    }
                    
                }

                foreach (var target in targets)
                {
                    if (target.TableControl.Parent != null && extendToAncestors)
                    {
                        target.TableControl.Parent.TableControl.Resizer.ResizeCell(target.TableControl.Parent);
                    }
                }

                OnResize?.Invoke(delta);
            }
        }
        
        protected void InvokeManualResize(HeaderControl target, float delta)
        {
            if(delta == 0) return;
            
            
            target.RegisterSingleUseCallback<GeometryChangedEvent>(_ =>
            {
                OnManualResize?.Invoke(delta);
            });
        }
        
        private void StartResize(PointerDownEvent downEvent)
        {
            if (resizingHeader == null 
                || excludedFromManualResizing.Contains(resizingHeader.Id) 
                || downEvent.button != 0 || tableControl is { enabledInHierarchy: false }
                || !IsResizingArea(downEvent.position, out _))
                return;
            
            IsResizing = true;
            var initialSize = new Vector2(resizingHeader.resolvedStyle.width, resizingHeader.resolvedStyle.height);
            var startPosition = downEvent.position;
            _newSize = initialSize;

            resizingPreview = new VisualElement();
            resizingPreview.AddToClassList(ResizingPreviewClass);
            tableControl.Add(resizingPreview);
            MovePreview(startPosition, initialSize, initialSize);

            tableControl.Root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            tableControl.Root.RegisterCallback<PointerUpEvent>(OnPointerUp);
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
                float delta = UpdateSize(resizingHeader, _newSize);
                UpdateChildrenSize(resizingHeader);
                InvokeResize(new List<HeaderControl>{resizingHeader}, delta, true, false, _newSize);
                InvokeManualResize(resizingHeader, delta);
                resizingPreview.RemoveFromHierarchy();
                
                tableControl.Root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                tableControl.Root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                IsResizing = false;
            }
        }
    }
}
