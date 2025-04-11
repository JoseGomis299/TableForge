using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class HorizontalBorderResizer : BorderResizer
    {
        protected override string ResizingPreviewClass => USSClasses.ResizePreviewHorizontal;

        public HorizontalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }

        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (ResizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in ResizingHeaders.Values)
            {
                float upperBound = headerControl.worldBound.yMax;
                float bottomBound = headerControl.worldBound.yMin;
                if (downEvent.position.y < bottomBound || downEvent.position.y > upperBound) continue;

                float rightBound = headerControl.worldBound.xMax;
                float leftBound = headerControl.worldBound.xMin;

                if (downEvent.position.x >= leftBound && downEvent.position.x <= rightBound)
                {
                    float delta = InstantResize(headerControl, false);
                    InvokeResize(headerControl, delta, true, false, Vector2.zero);
                    InvokeManualResize(headerControl, delta);
                    return;
                }
            }
        }

        protected override float InstantResize(HeaderControl target, bool fitStoredSize)
        {
            float targetWidth = TableControl.PreferredSize.GetHeaderSize(target.CellAnchor).x;

            if (fitStoredSize)
            {
                string anchorId = target.CellAnchor?.Id ?? TableControl.Parent?.Cell.Id ?? "";
                float storedWidth = TableControl.Metadata.GetAnchorSize(anchorId).x;
                if(storedWidth != 0) targetWidth = storedWidth;
            }

            float delta = UpdateSize(target, new Vector3(targetWidth, 0));
            UpdateChildrenSize(target);
            return delta;
        }

        protected override void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize)
        {
            if (ResizingPreview == null) return;

            var delta = newSize.x - initialSize.x;
            var position = startPosition.x - TableControl.worldBound.xMin + delta;
            ResizingPreview.style.left = position;
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;
            float margin = UiConstants.ResizableBorderSpan + UiConstants.BorderWidth / 2f;
            
            //If the mouse is touching the corner, we don't want to resize the columns behind it.
            if(position.x < TableControl.CornerContainer.worldBound.xMax - margin) return false;

            foreach (var header in ResizingHeaders.Values)
            {
                float upperBound = header.worldBound.yMax;
                float bottomBound = header.worldBound.yMin;
                if (position.y < bottomBound || position.y > upperBound) continue;

                float rightBound = header.worldBound.xMax;
                float minPosition = rightBound - margin;
                float maxPosition = rightBound + margin;

                if (position.x >= minPosition && position.x <= maxPosition)
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

            if(IsResizingArea(moveEvent.position, out var headerControl))
            {
                ResizingHeader = headerControl;

                foreach (var header in ResizingHeaders.Values)
                {
                    header.AddToClassList(USSClasses.CursorResizeHorizontal);
                    header.AddToChildrenClassList(USSClasses.CursorResizeHorizontal);
                }
                return;
            }
            
            if (IsResizing || ResizingHeader == null) return;

            foreach (var header in ResizingHeaders.Values)
            {
                header.RemoveFromClassList(USSClasses.CursorResizeHorizontal);
                header.RemoveFromChildrenClassList(USSClasses.CursorResizeHorizontal);
            }
            ResizingHeader = null;
        }
        
        protected override float UpdateSize(HeaderControl headerControl, Vector3 newSize)
        {
            float delta = newSize.x - headerControl.style.width.value.value;
            headerControl.style.width = newSize.x;
            return delta;
        }

        protected override void UpdateChildrenSize(HeaderControl headerControl)
        {
            if (headerControl is TableCornerControl cornerControl)
            {
                cornerControl.RowHeaderContainer.style.width = cornerControl.style.width;
                cornerControl.ColumnHeaderContainer.style.left = cornerControl.style.width;
                cornerControl.RowsContainer.style.left = cornerControl.style.width.value.value + TableControl.RowsContainerOffset;                
            }
            else
            {
                foreach (var child in TableControl.RowVisibilityManager.CurrentVisibleHeaders)
                {
                    child.RowControl.RefreshColumnWidths();
                }
            }
        }

        protected override Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition)
        {
            var delta = currentPosition.x - startPosition.x;

            float scaledWidth = initialSize.x + delta;
            float preferredWidth = TableControl.PreferredSize.GetHeaderSize(ResizingHeader.CellAnchor).x;
            
            if(scaledWidth >= preferredWidth - UiConstants.SnappingThreshold && scaledWidth <= preferredWidth + UiConstants.SnappingThreshold)
            {
                return new Vector3(preferredWidth, 0);
            }
            
            return new Vector3(Mathf.Max(UiConstants.MinCellWidth, scaledWidth), 0);
        }
    }
}