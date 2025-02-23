using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SwappingDragger : MouseManipulator
    {
        protected readonly TableControl TableControl;
        protected readonly Dictionary<int, Rect> HeaderBounds = new Dictionary<int, Rect>();

        protected Vector3 FinalPosition;
        protected int LastHeaderId;

        private bool _isDragging;

        protected SwappingDragger(TableControl tableControl)
        {
            TableControl = tableControl;
            activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse, clickCount = 1});
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }
        
        private void OnMouseDown(MouseDownEvent e)
        {
            FinalPosition= target.worldBound.position;
            LastHeaderId = 0;
            _isDragging = true;
            
            CacheHeaderBounds();
            target.CaptureMouse();
        }
        
        private void OnMouseUp(MouseUpEvent e)
        {
            if (!_isDragging) return;
           
            _isDragging = false;
            PerformSwap();

            target.ReleaseMouse();
        }
        
        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isDragging) return;

            MoveElements(e);
        }
        
        protected abstract void CacheHeaderBounds();
        protected abstract void MoveElements(MouseMoveEvent e);
        protected abstract void PerformSwap();
    }
}