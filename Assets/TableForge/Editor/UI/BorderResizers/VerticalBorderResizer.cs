using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class VerticalBorderResizer : BorderResizer
    {
        protected override string ResizingPreviewClass => USSClasses.ResizePreviewVertical;

        public VerticalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (ResizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in ResizingHeaders.Values)
            {
                float leftBound = headerControl.worldBound.xMin;
                float rightBound = headerControl.worldBound.xMax;
                if (downEvent.position.x < leftBound || downEvent.position.x > rightBound) continue;

                float bottomBound = headerControl.worldBound.yMin;
                float upperBound = headerControl.worldBound.yMax;

                if (downEvent.position.y >= bottomBound && downEvent.position.y <= upperBound)
                {
                    float delta = InstantResize(headerControl);
                    InvokeResize(headerControl, delta);
                    return;
                }
            }
        }

        protected override float InstantResize(HeaderControl target)
        {
            var rowData = TableControl.RowData[target.Id];
            float delta = UpdateSize(target, new Vector3(0, rowData.PreferredHeight));
            UpdateChildrenSize(target);
            return delta;
        }

        protected override void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize)
        {
            if (ResizingPreview == null) return;

            var delta = newSize.y - initialSize.y;
            var position = startPosition.y - TableControl.worldBound.yMin + delta;
            ResizingPreview.style.top = position;
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;
            float margin = UiConstants.ResizableBorderSpan + UiConstants.BorderWidth / 2f;

            //If the mouse is touching the corner, we don't want to resize the rows behind it.
            if(position.y < TableControl.CornerContainer.worldBound.yMax - margin) return false;

            foreach (var header in ResizingHeaders.Values)
            {
                float leftBound = header.worldBound.xMin;
                float rightBound = header.worldBound.xMax;
                if (position.x < leftBound || position.x > rightBound) continue;

                float bottomBound = header.worldBound.yMax;
                float minPosition = bottomBound - margin;
                float maxPosition = bottomBound + margin;

                if (position.y >= minPosition && position.y <= maxPosition)
                {
                    headerControl = header;
                    return true;
                }
            }

            return false;
        }

        protected override void CheckResize(PointerMoveEvent moveEvent)
        {
            if (ResizingHeaders.Count == 0 || IsResizing) return;

            if (IsResizingArea(moveEvent.position, out var headerControl))
            {
                ResizingHeader = headerControl;
                    
                foreach (var header in ResizingHeaders.Values)
                {
                    header.AddToClassList(USSClasses.CursorResizeVertical);
                    header.AddToChildrenClassList(USSClasses.CursorResizeVertical);
                }
                return;
            }

            if (IsResizing || ResizingHeader == null) return;
            
            foreach (var header in ResizingHeaders.Values)
            {
                header.RemoveFromClassList(USSClasses.CursorResizeVertical);
                header.RemoveFromChildrenClassList(USSClasses.CursorResizeVertical);
            }            
            ResizingHeader = null;
        }
        
        protected override float UpdateSize(HeaderControl headerControl, Vector3 newSize)
        {
            float delta = newSize.y - headerControl.style.height.value.value;
            headerControl.style.height = newSize.y;
            return delta;
        }

        protected override void UpdateChildrenSize(HeaderControl headerControl)
        {
            if (headerControl is RowHeaderControl rowHeaderControl)
            {
                rowHeaderControl.RowControl.style.height = rowHeaderControl.style.height;   
            }
        }

        protected override Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition)
        {
            var delta = currentPosition.y - startPosition.y;
            var rowData = TableControl.RowData[ResizingHeader.Id];
            
            float scaledHeight = initialSize.y + delta;
            float desiredHeight = rowData.PreferredHeight;
            
            if(scaledHeight >= desiredHeight - UiConstants.SnappingThreshold && scaledHeight <= desiredHeight + UiConstants.SnappingThreshold)
            {
                return new Vector3(0, desiredHeight);
            }
            
            return new Vector3(0, Mathf.Max(UiConstants.MinCellHeight, scaledHeight));
        }
    }
}