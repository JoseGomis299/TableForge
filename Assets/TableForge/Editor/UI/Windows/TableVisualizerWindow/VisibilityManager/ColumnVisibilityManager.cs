using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Manages the visibility of column headers in the table visualizer.
    /// Handles virtual scrolling for performance optimization with large tables.
    /// </summary>
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        #region Private Fields

        private const float SquareHorizontalStep = UiConstants.MinCellWidth * UiConstants.MinCellWidth;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ColumnVisibilityManager class.
        /// </summary>
        /// <param name="tableControl">The table control to manage column visibility for.</param>
        public ColumnVisibilityManager(TableControl tableControl) : base(tableControl)
        {
        }

        #endregion

        #region Public Methods - Event Subscription

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

        #endregion

        #region Public Methods - Visibility Management

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

        public override bool IsHeaderInBounds(ColumnHeaderControl header, bool addSecuritySize)
        {
           // if (tableControl.Parent is ExpandableSubTableCellControl { IsFoldoutOpen: true }) return true;
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
            float margin = 5;
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

        #endregion

        #region Private Methods - Event Handling

        private void OnScrollviewSizeChanged(Vector2 delta)
        {
            if (delta.x == 0 && delta.y != 0) return;

            RefreshVisibility(1);
        }

        private void OnHorizontalScroll(float value)
        {
            float delta = value - lastScrollValue;
            if (delta * delta < SquareHorizontalStep)
                return;

            lastScrollValue = value;
            RefreshVisibility(delta);
        }

        #endregion
    }
}