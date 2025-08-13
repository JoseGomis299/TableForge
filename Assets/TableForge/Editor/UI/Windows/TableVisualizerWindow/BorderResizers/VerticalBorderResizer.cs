using System.Collections.Generic;
using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.Editor.UI
{
    internal class VerticalBorderResizer : BorderResizer
    {
        protected override string ResizingPreviewClass => TableVisualizerUss.ResizePreviewVertical;

        public VerticalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (resizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in resizingHeaders.Values)
            {
                float leftBound = headerControl.worldBound.xMin;
                float rightBound = headerControl.worldBound.xMax;
                if (downEvent.position.x < leftBound || downEvent.position.x > rightBound) continue;

                float bottomBound = headerControl.worldBound.yMin;
                float upperBound = headerControl.worldBound.yMax;

                if (downEvent.position.y >= bottomBound && downEvent.position.y <= upperBound)
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
            float targetHeight = tableControl.PreferredSize.GetHeaderSize(target.CellAnchor).y;

            if (fitStoredSize)
            {
                int anchorId = target.CellAnchor?.Id ?? tableControl.Parent?.Cell.Id ?? 0;
                float storedHeight = tableControl.Metadata.GetAnchorSize(anchorId).y;
                if (storedHeight != 0) targetHeight = storedHeight;
            }
            
            float delta = UpdateSize(target, new Vector3(0, targetHeight));
            UpdateChildrenSize(target);
            return delta;
        }

        protected override void MovePreview(Vector3 startPosition, Vector3 initialSize, Vector3 newSize)
        {
            if (resizingPreview == null) return;

            var delta = newSize.y - initialSize.y;
            var position = startPosition.y - tableControl.worldBound.yMin + delta;
            resizingPreview.style.top = position;
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;
            float margin = UiConstants.ResizableBorderSpan + UiConstants.BorderWidth / 2f;

            //If the mouse is touching the corner, we don't want to resize the rows behind it.
            if(position.y < tableControl.CornerContainer.worldBound.yMax - margin) return false;
            if(!tableControl.ScrollView.contentViewport.worldBound.Contains(position)) return false;

            foreach (var header in resizingHeaders.Values)
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
            if (resizingHeaders.Count == 0 || IsResizing) return;

            if (IsResizingArea(moveEvent.position, out var headerControl))
            {
                if (excludedFromManualResizing.Contains(headerControl.Id)) return;
                resizingHeader = headerControl;
                    
                foreach (var header in resizingHeaders.Values)
                {
                    header.AddToClassList(TableVisualizerUss.CursorResizeVertical);
                    header.AddToChildrenClassList(TableVisualizerUss.CursorResizeVertical);
                }
                return;
            }

            if (IsResizing || resizingHeader == null) return;
            
            foreach (var header in resizingHeaders.Values)
            {
                header.RemoveFromClassList(TableVisualizerUss.CursorResizeVertical);
                header.RemoveFromChildrenClassList(TableVisualizerUss.CursorResizeVertical);
            }            
            resizingHeader = null;
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
            
            float scaledHeight = initialSize.y + delta;
            float preferredHeight = tableControl.PreferredSize.GetHeaderSize(resizingHeader.CellAnchor).y;
            
            if(scaledHeight >= preferredHeight - UiConstants.SnappingThreshold && scaledHeight <= preferredHeight + UiConstants.SnappingThreshold)
            {
                return new Vector3(0, preferredHeight);
            }
            
            return new Vector3(0, Mathf.Max(UiConstants.MinCellHeight, scaledHeight));
        }
    }
}