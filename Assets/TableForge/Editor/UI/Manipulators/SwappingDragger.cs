using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SwappingDragger : MouseManipulator
    {
        protected readonly TableControl TableControl;
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
            _isDragging = true;
            
            OnClick();
            target.CaptureMouse();
        }
        
        private void OnMouseUp(MouseUpEvent e)
        {
            if (!_isDragging) return;
           
            _isDragging = false;
            PerformSwap();

            OnRelease();
            target.ReleaseMouse();
        }
        
        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isDragging) return;

            MoveElements(e);
        }
        
        protected abstract void OnClick();
        protected abstract void OnRelease();
        protected abstract void MoveElements(MouseMoveEvent e);
        protected abstract void PerformSwap();
    }
}