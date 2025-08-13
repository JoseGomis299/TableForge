using UnityEngine;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Manages the visibility of row headers in the table visualizer.
    /// Handles virtual scrolling for performance optimization with large tables.
    /// </summary>
    internal class RowVisibilityManager : VisibilityManager<RowHeaderControl>
    {
        #region Private Fields

        private const float SquareVerticalStep = UiConstants.MinCellHeight * UiConstants.MinCellHeight;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RowVisibilityManager class.
        /// </summary>
        /// <param name="tableControl">The table control to manage row visibility for.</param>
        public RowVisibilityManager(TableControl tableControl) : base(tableControl)
        {
        }

        #endregion

        #region Public Methods - Event Subscription

        public override void SubscribeToRefreshEvents()
        {
            scrollView.verticalScroller.valueChanged += OnVerticalScroll;
            tableControl.OnScrollviewSizeChanged += OnScrollviewSizeChanged;
        }

        public override void UnsubscribeFromRefreshEvents()
        {
            scrollView.verticalScroller.valueChanged -= OnVerticalScroll;
            tableControl.OnScrollviewSizeChanged -= OnScrollviewSizeChanged;
        }

        #endregion

        #region Public Methods - Visibility Management

        public override void RefreshVisibility(float delta)
        {
            if(IsRefreshingVisibility || tableControl.ColumnVisibilityManager.IsRefreshingVisibility || tableControl.RowData.Count <= 1) return;
            IsRefreshingVisibility = true;
            int direction = delta > 0 ? -1 : 1;
            bool isScrollingDown = direction == -1;

            UpdatePreviouslyVisibleRows(isScrollingDown);
            
            foreach (var rowHeader in tableControl.OrderedRowHeaders)
            {
                if (tableControl.Filterer.IsVisible(rowHeader.CellAnchor.GetRootAnchor().Id) && (rowHeader.IsVisible || IsHeaderVisible(rowHeader)))
                {
                    MakeHeaderVisible(rowHeader, insertAtTop: false);
                }
            }

            SendVisibilityNotifications(direction);
            IsRefreshingVisibility = false;
        }

        public override bool IsHeaderInBounds(RowHeaderControl header, bool addSecuritySize)
        {
        //    if (tableControl.Parent is ExpandableSubTableCellControl { IsFoldoutOpen: true }) return true;
            if(header.worldBound.height <= 0)
                return false;
            
            Vector2 securitySize = addSecuritySize ? new Vector2(0, securityExtraSize.y) : Vector2.zero;
            var viewBounds = scrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.width, viewBounds.height - tableControl.CornerContainer.CornerControl.resolvedStyle.height) + securitySize;
            viewBounds.y += tableControl.CornerContainer.CornerControl.resolvedStyle.height - securitySize.y / 2f;

            // Check if the top of the header is visible.
            if (header.worldBound.yMax <= viewBounds.yMax &&
                header.worldBound.yMax >= viewBounds.yMin)
                return true;

            // Check if the bottom of the header is visible.
            if (header.worldBound.yMin >= viewBounds.yMin &&
                header.worldBound.yMin <= viewBounds.yMax)
                return true;

            // Check if the header completely covers the visible area.
            return header.worldBound.yMax >= viewBounds.yMax &&
                   header.worldBound.yMin <= viewBounds.yMin;
        }
        
        public override bool IsHeaderCompletelyInBounds(RowHeaderControl header, bool addSecuritySize, out sbyte visibleBounds)
        {
            float margin = 5;
            Vector2 securitySize = addSecuritySize ? new Vector2(0, securityExtraSize.y) : Vector2.zero;
            var viewBounds = scrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.width, viewBounds.height - tableControl.CornerContainer.CornerControl.resolvedStyle.height) + securitySize;
            viewBounds.y += tableControl.CornerContainer.CornerControl.resolvedStyle.height;
            
            bool isBottomSideVisible = header.worldBound.yMax - viewBounds.yMax <= margin &&
                                    header.worldBound.yMax - viewBounds.yMin >= -margin;
            
            bool isTopSideVisible = header.worldBound.yMin - viewBounds.yMin >= -margin &&
                                       header.worldBound.yMin - viewBounds.yMax <= margin;

            visibleBounds = 0;
            if (isBottomSideVisible) visibleBounds += 1;
            if (isTopSideVisible) visibleBounds += 2;
            
            return isTopSideVisible && isBottomSideVisible;
        }

        #endregion

        #region Private Methods - Event Handling

        private void OnScrollviewSizeChanged(Vector2 delta)
        {
            if (delta.y == 0 && delta.x != 0) return;

            RefreshVisibility(delta.y);
        }

        private void OnVerticalScroll(float value)
        {
            float delta = value - lastScrollValue;
            if (delta * delta < SquareVerticalStep)
                return;

            lastScrollValue = value;
            RefreshVisibility(delta);
        }

        #endregion

        #region Private Methods - Visibility Updates

        private void UpdatePreviouslyVisibleRows(bool isScrollingDown)
        {
            if (isScrollingDown)
            {
                /*
                 * As we are scrolling down, we can assume that from the first visible row we find,
                 * all the following rows are visible. So when we reach that point, we can stop checking.
                 */
                
                bool firstVisibleFound = false;
                foreach (var header in visibleHeaders)
                {
                    if(!tableControl.Filterer.IsVisible(header.CellAnchor.GetRootAnchor().Id)) continue;
                    
                    if (!firstVisibleFound)
                    {
                        bool wasVisible = header.IsVisible && !lockedVisibleHeaders.ContainsKey(header);
                        header.IsVisible = IsHeaderVisible(header);
                        firstVisibleFound = header.IsVisible && !lockedVisibleHeaders.ContainsKey(header);
                        if (!firstVisibleFound && wasVisible)
                            MakeHeaderInvisible(header);
                    }
                    else
                    {
                        header.IsVisible = true;
                    }
                }
            }
            else
            {
                /*
                 * As we are scrolling up, we can assume that from the last visible row we find,
                 * all the previous rows are visible. So when we reach that point, we can stop checking.
                 */

                bool lastVisibleFound = false;
                for (int i = visibleHeaders.Count - 1; i >= 0; i--)
                {
                    var header = visibleHeaders[i];
                    if(!tableControl.Filterer.IsVisible(header.CellAnchor.GetRootAnchor().Id)) continue;

                    if (!lastVisibleFound)
                    {
                        bool wasVisible = header.IsVisible && !lockedVisibleHeaders.ContainsKey(header);
                        header.IsVisible = IsHeaderVisible(header);
                        lastVisibleFound = header.IsVisible && !lockedVisibleHeaders.ContainsKey(header);
                        if (!lastVisibleFound && wasVisible)
                            MakeHeaderInvisible(header);
                    }
                    else
                    {
                        header.IsVisible = true;
                    }
                }
            }
            visibleHeaders.Clear();
        }

        #endregion
    }
}