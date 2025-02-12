using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace TableForge.UI
{
    internal class HorizontalBorderResizer : BorderResizer
    {
        public HorizontalBorderResizer(TableControl tableControl) : base(tableControl)
        {
        }
        
        protected override void HandleDoubleClick(PointerDownEvent downEvent)
        {
            if (ResizingHeaders.Count == 0 || IsResizing || downEvent.button != 0) return;
            if (downEvent.clickCount != 2) return;

            foreach (var headerControl in ResizingHeaders)
            {
                float upperBound = headerControl.worldBound.yMax;
                float bottomBound = headerControl.worldBound.yMin;
                if (downEvent.position.y < bottomBound || downEvent.position.y > upperBound) continue;

                float rightBound = headerControl.worldBound.xMax;
                float leftBound = headerControl.worldBound.xMin;

                if (downEvent.position.x >= leftBound && downEvent.position.x <= rightBound)
                {
                    InstantResize(headerControl);
                    return;
                }
            }
        }

        protected override void InstantResize(HeaderControl target)
        {
            if(target.Id == 0) return;
            var columnData = TableControl.ColumnData[target.Id];
            target.style.width = columnData.PreferredWidth;
            UpdateChildrenSize(target, columnData.PreferredWidth);
        }

        public override bool IsResizingArea(Vector3 position, out HeaderControl headerControl)
        {
            headerControl = null;

            foreach (var header in ResizingHeaders)
            {
                float upperBound = header.worldBound.yMax;
                float bottomBound = header.worldBound.yMin;
                if (position.y < bottomBound || position.y > upperBound) continue;

                float rightBound = header.worldBound.xMax;
                float margin = UiContants.ResizableBorderSpan + UiContants.BorderWidth / 2f;
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

                foreach (var header in ResizingHeaders)
                {
                    header.AddToClassList("cursor__resize--horizontal");
                    header.AddToChildrenClassList("cursor__resize--horizontal");
                }
                return;
            }
            
            if (IsResizing || ResizingHeader == null) return;

            foreach (var header in ResizingHeaders)
            {
                header.RemoveFromClassList("cursor__resize--horizontal");
                header.RemoveFromChildrenClassList("cursor__resize--horizontal");
            }
            ResizingHeader = null;
        }

        protected override void UpdateChildrenSize(HeaderControl headerControl, float newWidth)
        {
            headerControl.style.width = newWidth;

            foreach (var child in TableControl.RowsContainer.Children())
            {
                if (child is TableRowControl rowControl)
                {
                    rowControl.SetColumnWidth(headerControl.Id, newWidth);
                }
            }
        }

        protected override float CalculateNewSize(Vector2 initialSize, Vector3 startPosition, Vector3 currentPosition)
        {
            var delta = currentPosition.x - startPosition.x;
            var columnData = TableControl.ColumnData[ResizingHeader.Id];

            float scaledWidth = initialSize.x + delta;
            float desiredWidth = columnData.PreferredWidth;
            
            if(scaledWidth >= desiredWidth - UiContants.SnappingThreshold && scaledWidth <= desiredWidth + UiContants.SnappingThreshold)
            {
                return desiredWidth;
            }
            
            return Mathf.Max(UiContants.MinCellWidth, scaledWidth);
        }
    }
}