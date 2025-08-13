using System.Collections.Generic;
using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.Editor.UI
{
    internal class HorizontalBorderResizer : BorderResizer
    {
        protected override string ResizingPreviewClass => TableVisualizerUss.ResizePreviewHorizontal;

        public HorizontalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }

        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (resizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in resizingHeaders.Values)
            {
                float upperBound = headerControl.worldBound.yMax;
                float bottomBound = headerControl.worldBound.yMin;
                if (downEvent.position.y < bottomBound || downEvent.position.y > upperBound) continue;

                float rightBound = headerControl.worldBound.xMax;
                float leftBound = headerControl.worldBound.xMin;

                if (downEvent.position.x >= leftBound && downEvent.position.x <= rightBound)
                {
                    if(excludedFromManualResizing.Contains(headerControl.Id)) return;

                    float delta = InstantResize(headerControl, false);
                    InvokeResize(new List<HeaderControl>{headerControl}, delta, true, false, Vector2.zero);
                    InvokeManualResize(headerControl, delta);
                    return;
                }
            }
        }

        protected override float InstantResize(HeaderControl target, bool fitStoredSize)
        {
            float targetWidth = tableControl.PreferredSize.GetHeaderSize(target.CellAnchor).x;

            if (fitStoredSize)
            {
                int anchorId = target.CellAnchor?.Id ?? tableControl.Parent?.Cell.Id ?? 0;
                float storedWidth = tableControl.Metadata.GetAnchorSize(anchorId).x;
                if(storedWidth != 0) targetWidth = storedWidth;
            }

            float delta = UpdateSize(target, new Vector3(targetWidth, 0));
            UpdateChildrenSize(target);
            return delta;
        }

        protected override void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize)
        {
            if (resizingPreview == null) return;

            var delta = newSize.x - initialSize.x;
            var position = startPosition.x - tableControl.worldBound.xMin + delta;
            resizingPreview.style.left = position;
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;
            float margin = UiConstants.ResizableBorderSpan + UiConstants.BorderWidth / 2f;
            
            //If the mouse is touching the corner, we don't want to resize the columns behind it.
            if(position.x < tableControl.CornerContainer.worldBound.xMax - margin) return false;
            if(!tableControl.ScrollView.contentViewport.worldBound.Contains(position)) return false;

            foreach (var header in resizingHeaders.Values)
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
            if (resizingHeaders.Count == 0 || IsResizing) return;

            if(IsResizingArea(moveEvent.position, out var headerControl))
            {
                if (excludedFromManualResizing.Contains(headerControl.Id)) return;
                resizingHeader = headerControl;

                foreach (var header in resizingHeaders.Values)
                {
                    header.AddToClassList(TableVisualizerUss.CursorResizeHorizontal);
                    header.AddToChildrenClassList(TableVisualizerUss.CursorResizeHorizontal);
                }
                return;
            }
            
            if (IsResizing || resizingHeader == null) return;

            foreach (var header in resizingHeaders.Values)
            {
                header.RemoveFromClassList(TableVisualizerUss.CursorResizeHorizontal);
                header.RemoveFromChildrenClassList(TableVisualizerUss.CursorResizeHorizontal);
            }
            resizingHeader = null;
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
                cornerControl.RowsContainer.style.left = cornerControl.style.width.value.value + tableControl.RowsContainerOffset;                
            }
            else
            {
                foreach (var child in tableControl.RowVisibilityManager.CurrentVisibleHeaders)
                {
                    child.RowControl.RefreshColumnWidths();
                }
            }
        }

        protected override Vector3 CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition)
        {
            var delta = currentPosition.x - startPosition.x;

            float scaledWidth = initialSize.x + delta;
            float preferredWidth = tableControl.PreferredSize.GetHeaderSize(resizingHeader.CellAnchor).x;
            
            if(scaledWidth >= preferredWidth - UiConstants.SnappingThreshold && scaledWidth <= preferredWidth + UiConstants.SnappingThreshold)
            {
                return new Vector3(preferredWidth, 0);
            }
            
            return new Vector3(Mathf.Max(UiConstants.MinCellWidth, scaledWidth), 0);
        }
    }
}