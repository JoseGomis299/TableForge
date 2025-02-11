using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class VerticalBorderResizer : BorderResizer
    {
        public VerticalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (ResizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in ResizingHeaders)
            {
                float leftBound = headerControl.worldBound.xMin;
                float rightBound = headerControl.worldBound.xMax;
                if (downEvent.position.x < leftBound || downEvent.position.x > rightBound) continue;

                float bottomBound = headerControl.worldBound.yMin;
                float upperBound = headerControl.worldBound.yMax;

                if (downEvent.position.y >= bottomBound && downEvent.position.y <= upperBound)
                {
                    InstantResize(headerControl);
                    return;
                }
            }
        }

        protected override void InstantResize(HeaderControl target)
        {
            var rowData = TableControl.RowData[target.Id];
            target.style.height = rowData.PreferredHeight;
            UpdateChildrenSize(target, rowData.PreferredHeight);
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;

            foreach (var header in ResizingHeaders)
            {
                float leftBound = header.worldBound.xMin;
                float rightBound = header.worldBound.xMax;
                if (position.x < leftBound || position.x > rightBound) continue;

                float bottomBound = header.worldBound.yMax;
                float margin = UiContants.ResizableBorderSpan + UiContants.BorderWidth / 2f;
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
                    
                foreach (var header in ResizingHeaders)
                {
                    header.AddToClassList("cursor__resize--vertical");
                    header.AddToChildrenClassList("cursor__resize--vertical");
                }
                return;
            }

            if (IsResizing || ResizingHeader == null) return;
            
            foreach (var header in ResizingHeaders)
            {
                header.RemoveFromClassList("cursor__resize--vertical");
                header.RemoveFromChildrenClassList("cursor__resize--vertical");
            }            
            ResizingHeader = null;
        }

        protected override void UpdateChildrenSize(HeaderControl headerControl, float newHeight)
        {
            headerControl.style.height = newHeight;

            foreach (var child in TableControl.RowsContainer.Children())
            {
                if (child is TableRowControl rowControl)
                {
                    if (rowControl.Children().First() != headerControl) continue;

                    rowControl.style.height = newHeight;
                    return;
                }
            }
        }

        protected override float CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition)
        {
            var delta = currentPosition.y - startPosition.y;
            var rowData = TableControl.RowData[ResizingHeader.Id];
            
            float scaledHeight = initialSize.y + delta;
            float desiredHeight = rowData.PreferredHeight;
            
            if(scaledHeight >= desiredHeight - UiContants.SnappingThreshold && scaledHeight <= desiredHeight + UiContants.SnappingThreshold)
            {
                return desiredHeight;
            }
            
            return Mathf.Max(UiContants.MinCellHeight, scaledHeight);
        }
    }
}