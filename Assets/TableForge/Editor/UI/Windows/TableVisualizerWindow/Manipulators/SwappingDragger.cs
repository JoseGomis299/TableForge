using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class SwappingDragger : MouseManipulator
    {
        protected readonly TableControl tableControl;
        private bool _isDragging;

        protected SwappingDragger(TableControl tableControl)
        {
            this.tableControl = tableControl;
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
        
        public void Dispose()
        {
            UnregisterCallbacksFromTarget();
            target.RemoveManipulator(this);
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

            if (e.pressedButtons != 1)
                return;
            
            MoveElements(e);
        }
        
        protected abstract void OnClick();
        protected abstract void OnRelease();
        protected abstract void MoveElements(MouseMoveEvent e);
        protected abstract void PerformSwap();
    }
}