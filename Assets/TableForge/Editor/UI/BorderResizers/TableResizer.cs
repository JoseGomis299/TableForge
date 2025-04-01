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
                Debug.Log(TableControl.TableData.Name + " is resizing");
                _resizeQueue.Enqueue(() => ResizeAll(adjustToStoredSize));
                return;
            };
            
            float horizontalDelta = HorizontalResizer.ResizeAll(adjustToStoredSize);
            float verticalDelta = VerticalResizer.ResizeAll(adjustToStoredSize);
            _currentDelta = new Vector2(horizontalDelta, verticalDelta);

            if(horizontalDelta != 0)
            {
                HorizontalResizer.OnResize += OnHorizontalResizeComplete;
                _horizontalIsResizing = true;
            }
            if(verticalDelta != 0)
            {
                VerticalResizer.OnResize += OnVerticalResizeComplete;
                _verticalIsResizing = true;
            }
        }
        
        public void ResizeCell(CellControl cellControl, bool storeSize = true)
        {
            if(IsResizing) 
            {
                Debug.Log(TableControl.TableData.Name + " is resizing");
                _resizeQueue.Enqueue(() => ResizeCell(cellControl, storeSize));
                return;
            }
            
            float horizontalDelta = HorizontalResizer.ResizeCell(cellControl, storeSize);
            float verticalDelta = VerticalResizer.ResizeCell(cellControl, storeSize);
            _currentDelta = new Vector2(horizontalDelta, verticalDelta);
            
            if(horizontalDelta != 0)
            {
                HorizontalResizer.OnResize += OnHorizontalResizeComplete;
                _horizontalIsResizing = true;
            }
            if(verticalDelta != 0)
            {
                VerticalResizer.OnResize += OnVerticalResizeComplete;
                _verticalIsResizing = true;
            }

            TableControl.Parent?.TableControl.Resizer.ResizeCell(cellControl.TableControl.Parent, storeSize);
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
            if (!_horizontalIsResizing)
            {
                _horizontalIsResizing = _verticalIsResizing = false;
                OnResize?.Invoke(_currentDelta);
                
                if(_resizeQueue.Count > 0)
                {
                    Debug.Log("Invoking next resize");
                    _resizeQueue.Dequeue().Invoke();
                }
            }
               
            VerticalResizer.OnResize -= OnVerticalResizeComplete;
            _verticalIsResizing = false;
        }

        private void OnHorizontalResizeComplete(float delta)
        {
            if (!_verticalIsResizing)
            {
                _horizontalIsResizing = _verticalIsResizing = false;
                OnResize?.Invoke(_currentDelta);
                
                if(_resizeQueue.Count > 0)
                {
                    Debug.Log("Invoking next resize");
                    _resizeQueue.Dequeue().Invoke();
                }
            }
                
            HorizontalResizer.OnResize -= OnHorizontalResizeComplete;
            _horizontalIsResizing = false;
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