using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Manages table resizing operations by coordinating horizontal and vertical border resizers.
    /// Handles resize queuing, completion callbacks, and manual resize events.
    /// </summary>
    internal class TableResizer
    {
        #region Events

        /// <summary>
        /// Event fired when a resize operation completes.
        /// </summary>
        public event Action<Vector2> OnResize;
        
        /// <summary>
        /// Event fired when a manual resize operation occurs.
        /// </summary>
        public event Action<Vector2> OnManualResize;

        #endregion

        #region Public Properties

        public HorizontalBorderResizer HorizontalResizer { get; }
        public VerticalBorderResizer VerticalResizer { get; }
        public TableControl TableControl { get; }
        public bool IsResizing => _horizontalIsResizing || _verticalIsResizing;

        #endregion

        #region Private Fields

        /// <summary>
        /// Queue of resize operations to be processed.
        /// </summary>
        private readonly Queue<Action> _resizeQueue = new();
        
        private Vector2 _currentDelta;    
        private bool _horizontalIsResizing;
        private bool _verticalIsResizing;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the TableResizer class.
        /// </summary>
        /// <param name="tableControl">The table control to resize.</param>
        public TableResizer(TableControl tableControl)
        {
            TableControl = tableControl;
            HorizontalResizer = new HorizontalBorderResizer(tableControl);
            VerticalResizer = new VerticalBorderResizer(tableControl);
            
            // Subscribe to resize events from both resizers
            HorizontalResizer.OnResize += InvokeResizeFromHorizontal;
            HorizontalResizer.OnManualResize += InvokeManualResizeFromHorizontal;
            VerticalResizer.OnResize += InvokeResizeFromVertical;
            VerticalResizer.OnManualResize += InvokeManualResizeFromVertical;
        }

        #endregion

        #region Public Methods - State Management

        /// <summary>
        /// Clears the resize queue and resets the resizing state.
        /// </summary>
        public void Clear()
        {
            _resizeQueue.Clear();
            _verticalIsResizing = _horizontalIsResizing = false;
        }

        #endregion

        #region Public Methods - Resize Operations

        /// <summary>
        /// Resizes all cells in the table to fit their stored sizes or content.
        /// </summary>
        /// <param name="fitStoredSize">Whether to use stored size information.</param>
        public void ResizeAll(bool fitStoredSize, bool storeSize = false)
        {
            if (IsResizing)
            {
                _resizeQueue.Enqueue(() => ResizeAll(fitStoredSize, storeSize));
                return;
            };

            HorizontalResizer.OnResize += OnHorizontalResizeComplete;
            _horizontalIsResizing = true;

            float horizontalDelta = HorizontalResizer.ResizeAll(fitStoredSize, storeSize);
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
                _currentDelta.y = VerticalResizer.ResizeAll(fitStoredSize, storeSize);
                
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
        
        /// <summary>
        /// Resizes a specific cell to fit its content or stored size.
        /// </summary>
        /// <param name="cellControl">The cell control to resize.</param>
        /// <param name="storeSize">Whether to store the new size.</param>
        public void ResizeCell(CellControl cellControl, bool storeSize = true)
        {
            if(IsResizing) 
            {
                _resizeQueue.Enqueue(() => ResizeCell(cellControl, storeSize));
                return;
            }

            HorizontalResizer.OnResize += OnHorizontalResizeComplete;
            _horizontalIsResizing = true;
            
            float horizontalDelta = HorizontalResizer.ResizeCell(cellControl, storeSize);
            _currentDelta = new Vector2(horizontalDelta, 0);

            if(horizontalDelta == 0)
            {
                HorizontalResizer.OnResize -= OnHorizontalResizeComplete;
                
                _horizontalIsResizing = true;
                OnHorizontalResizeComplete(0);
            }

            // Propagate resize to parent table if this is a sub-table
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

        #endregion

        #region Private Methods - Event Handling

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

        #endregion

        #region Private Methods - Event Invocation
        
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

        #endregion
    }
}