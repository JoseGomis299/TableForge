using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        private const float SquareHorizontalStep = UiConstants.MinCellWidth * UiConstants.MinCellWidth;

        public ColumnVisibilityManager(TableControl tableControl) : base(tableControl)
        {
        }

        public override void SubscribeToRefreshEvents()
        {
            scrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            tableControl.OnScrollviewSizeChanged += OnScrollviewSizeChanged;
        }

        public override void UnsubscribeFromRefreshEvents()
        {
            scrollView.horizontalScroller.valueChanged -= OnHorizontalScroll;
            tableControl.OnScrollviewSizeChanged -= OnScrollviewSizeChanged;
        }
        
        private void OnScrollviewSizeChanged(Vector2 delta)
        {
            if (delta.x == 0 && delta.y != 0) return;

            RefreshVisibility(1);
        }

        public override void RefreshVisibility(float delta)
        {
            if(IsRefreshingVisibility 
               || tableControl.RowVisibilityManager.IsRefreshingVisibility
               || tableControl.ColumnData.Count <= 1
               || tableControl.Parent is ExpandableSubTableCellControl { IsFoldoutOpen: false })
                return;
            IsRefreshingVisibility = true;
            int direction = delta > 0 ? 1 : -1;
            
            // Update visibility of columns that were previously visible.
            foreach (var header in visibleHeaders)
            {
                bool wasVisible = header.IsVisible && !lockedVisibleHeaders.ContainsKey(header);
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible && wasVisible)
                    MakeHeaderInvisible(header);
            }

            visibleHeaders.Clear();
            
            var orderedColumnHeaders = direction == 1 ?
                tableControl.OrderedColumnHeaders : 
                tableControl.OrderedDescColumnHeaders;

            // Loop through all column headers.
            foreach (var header in orderedColumnHeaders)
            {
                if (header.IsVisible || IsHeaderVisible(header))
                {
                    MakeHeaderVisible(header, insertAtTop: false);
                }
            }
            
            SendVisibilityNotifications(direction);
            IsRefreshingVisibility = false;
        }

        private void OnHorizontalScroll(float value)
        {
            float delta = value - lastScrollValue;
            if (delta * delta < SquareHorizontalStep)
                return;

            lastScrollValue = value;
            RefreshVisibility(delta);
        }

        public override bool IsHeaderInBounds(ColumnHeaderControl header, bool addSecuritySize)
        {
            if(header.worldBound.width <= 0)
                return false;
            
            Vector2 securitySize = addSecuritySize ? new Vector2(securityExtraSize.x, 0) : Vector2.zero;
            var viewBounds = scrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x - tableControl.CornerContainer.worldBound.width, viewBounds.size.y) + securitySize / 2f;
            viewBounds.x += tableControl.CornerContainer.worldBound.width - securitySize.x / 2f;
            
            // Check if the left side of the header is visible.
            if (header.worldBound.xMin <= viewBounds.xMax &&
                header.worldBound.xMin >= viewBounds.xMin)
                return true;

            // Check if the right side of the header is visible.
            if (header.worldBound.xMax >= viewBounds.xMin &&
                header.worldBound.xMax <= viewBounds.xMax)
                return true;

            // Check if the header completely covers the visible area.
            return header.worldBound.xMax >= viewBounds.xMax &&
                   header.worldBound.xMin <= viewBounds.xMin;
        }
        
        public override bool IsHeaderCompletelyInBounds(ColumnHeaderControl header, bool addSecuritySize, out sbyte visibleBounds)
        {
            float margin = 1;
            Vector2 securitySize = addSecuritySize ? new Vector2(securityExtraSize.x, 0) : Vector2.zero;
            var viewBounds = scrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x - tableControl.CornerContainer.worldBound.width, viewBounds.size.y) + securitySize / 2f;
            viewBounds.x += tableControl.CornerContainer.worldBound.width - securitySize.x / 2f;

            bool isLeftSideVisible = header.worldBound.xMin - viewBounds.xMax <= margin &&
                                     header.worldBound.xMin - viewBounds.xMin >= -margin;

            bool isRightSideVisible = header.worldBound.xMax - viewBounds.xMin >= -margin &&
                                      header.worldBound.xMax - viewBounds.xMax <= margin;

            visibleBounds = 0;
            if (isLeftSideVisible) visibleBounds += 1;
            if (isRightSideVisible) visibleBounds += 2;

            return isLeftSideVisible && isRightSideVisible;
        }
    }
}