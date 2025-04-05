using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.UI
{
    internal class TableResizer
    {
        public event Action<Vector2> OnResize;
        public event Action<Vector2> OnManualResize;
        
        public HorizontalBorderResizer HorizontalResizer { get; }
        public VerticalBorderResizer VerticalResizer { get; }
        public TableControl TableControl { get; }
        
        private Queue<Action> _resizeQueue = new Queue<Action>();
        
        public bool IsResizing => _horizontalIsResizing || _verticalIsResizing;
        
        private Vector2 _currentDelta;
        private bool _horizontalIsResizing;
        private bool _verticalIsResizing;
        
        public TableResizer(TableControl tableControl)
        {
            TableControl = tableControl;
            HorizontalResizer = new HorizontalBorderResizer(tableControl);
            VerticalResizer = new VerticalBorderResizer(tableControl);
            
            HorizontalResizer.OnResize += InvokeResizeFromHorizontal;
            HorizontalResizer.OnManualResize += InvokeManualResizeFromHorizontal;
            VerticalResizer.OnResize += InvokeResizeFromVertical;
            VerticalResizer.OnManualResize += InvokeManualResizeFromVertical;
        }
        
        public void Clear()
        {
            _resizeQueue.Clear();
            _verticalIsResizing = _horizontalIsResizing = false;
        }
        
        public void ResizeAll(bool adjustToStoredSize)
        {
            if (IsResizing)
            {
                _resizeQueue.Enqueue(() => ResizeAll(adjustToStoredSize));
                return;
            };

            HorizontalResizer.OnResize += OnHorizontalResizeComplete;
            _horizontalIsResizing = true;

            float horizontalDelta = HorizontalResizer.ResizeAll(adjustToStoredSize);
            _currentDelta = new Vector2(horizontalDelta, 0);

            if(horizontalDelta == 0)
            {
                HorizontalResizer.OnResize -= OnHorizontalResizeComplete;
                
                _horizontalIsResizing = true;
                OnHorizontalResizeComplete(0);
            }
            
            void OnHorizontalResizeComplete(float delta)
            {
                if(!_horizontalIsResizing) return;
                
                HorizontalResizer.OnResize -= OnHorizontalResizeComplete;
                _horizontalIsResizing = false;
                _currentDelta.x = delta;
                
                VerticalResizer.OnResize += OnVerticalResizeComplete;
                _verticalIsResizing = true;
                _currentDelta.y = VerticalResizer.ResizeAll(adjustToStoredSize);
                
                if(_currentDelta.y == 0)
                {
                    VerticalResizer.OnResize -= OnVerticalResizeComplete;
                    _verticalIsResizing = false;
                    
                    OnResize?.Invoke(_currentDelta);
                
                    if(_resizeQueue.Count > 0)
                    {
                        _resizeQueue.Dequeue().Invoke();
                    }
                }
            }
        }
        
        public void ResizeCell(CellControl cellControl, bool storeSize = true)
        {
            if(IsResizing) 
            {
                _resizeQueue.Enqueue(() => ResizeCell(cellControl, storeSize));
                return;
            }

            float horizontalDelta = HorizontalResizer.ResizeCell(cellControl, storeSize);
            _currentDelta = new Vector2(horizontalDelta, 0);

            if(horizontalDelta != 0)
            {
                HorizontalResizer.OnResize += OnHorizontalResizeComplete;
                _horizontalIsResizing = true;
            }
            else
            {
                _horizontalIsResizing = true;
                OnHorizontalResizeComplete(0);
            }

            TableControl.Parent?.TableControl.Resizer.ResizeCell(cellControl.TableControl.Parent, storeSize);
            
            void OnHorizontalResizeComplete(float delta)
            {
                if(!_horizontalIsResizing) return;
                
                HorizontalResizer.OnResize -= OnHorizontalResizeComplete;
                _horizontalIsResizing = false;
                _currentDelta.x = delta;
                
                VerticalResizer.OnResize += OnVerticalResizeComplete;
                _verticalIsResizing = true;
                _currentDelta.y = VerticalResizer.ResizeCell(cellControl, storeSize);
                
                if(_currentDelta.y == 0)
                {
                    VerticalResizer.OnResize -= OnVerticalResizeComplete;
                    _verticalIsResizing = false;
                    
                    OnResize?.Invoke(_currentDelta);
                
                    if(_resizeQueue.Count > 0)
                    {
                        _resizeQueue.Dequeue().Invoke();
                    }
                }
            }
        }
        
        private void InvokeResizeFromVertical(float delta)
        {
            InvokeResize(new Vector2(0, delta));
        }
        
        private void InvokeResizeFromHorizontal(float delta)
        {
           InvokeResize(new Vector2(delta, 0));
        }
        
        private void InvokeManualResizeFromVertical(float delta)
        {
            InvokeManualResize(new Vector2(0, delta));
        }
        
        private void InvokeManualResizeFromHorizontal(float delta)
        {
            InvokeManualResize( new Vector2(delta, 0));
        }
        
        private void OnVerticalResizeComplete(float delta)
        {
            VerticalResizer.OnResize -= OnVerticalResizeComplete;
            _verticalIsResizing = false;
            _currentDelta.y = delta;

            OnResize?.Invoke(_currentDelta);
                
            if(_resizeQueue.Count > 0)
            {
                _resizeQueue.Dequeue().Invoke();
            }
        }
        
        private void InvokeResize(Vector2 delta)
        {
            if(IsResizing)
            {
                return;
            }
            
            _horizontalIsResizing = _verticalIsResizing = false;
            OnResize?.Invoke(delta);
        }
        
        private void InvokeManualResize(Vector2 delta)
        {
            if(IsResizing)
            {
                return;
            }
            
            _horizontalIsResizing = _verticalIsResizing = false;
            OnManualResize?.Invoke(delta);
        }
        
        
        
    }
}