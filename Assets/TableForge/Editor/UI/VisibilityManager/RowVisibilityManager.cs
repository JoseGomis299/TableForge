using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class RowVisibilityManager : VisibilityManager<RowHeaderControl>
    {
        private const float SQUARE_VERTICAL_STEP = UiConstants.MinCellHeight * UiConstants.MinCellHeight;
        private const int SECURITY_MARGIN = 1;

        private readonly TableControl _tableControl;
        private float _lastVerticalScroll;

        public RowVisibilityManager(TableControl tableControl, ScrollView scrollView) : base(scrollView)
        {
            _tableControl = tableControl;
            _lastVerticalScroll = float.MinValue;

            // Subscribe to vertical scroll changes.
            _scrollView.verticalScroller.valueChanged += OnVerticalScroll;
            _tableControl.OnScrollviewHeightChanged += () => RefreshVisibility(_scrollView.verticalScroller.value);
            _tableControl.RegisterCallback<GeometryChangedEvent>(_ => RefreshVisibility(_scrollView.verticalScroller.value));

        }
        
        private void OnVerticalScroll(float value)
        {
            float delta = value - _lastVerticalScroll;
            if (delta * delta < SQUARE_VERTICAL_STEP)
                return;

            _lastVerticalScroll = value;
            RefreshVisibility(value);
        }

        protected override void NotifyHeaderBecameVisible(RowHeaderControl header)
        {
            header.RowControl.RefreshColumnWidths();
            header.RemoveFromClassList(USSClasses.Hidden);
            header.RowControl.RemoveFromClassList(USSClasses.Hidden);
            
            base.NotifyHeaderBecameVisible(header);
        }

        protected override void NotifyHeaderBecameInvisible(RowHeaderControl header)
        {
            header.AddToClassList(USSClasses.Hidden);
            header.RowControl.AddToClassList(USSClasses.Hidden);

            base.NotifyHeaderBecameInvisible(header);
        }

        public override void RefreshVisibility(float value)
        {
            bool isScrollingDown = value > 0;

            // Update visibility of rows that were previously visible based on scroll direction.
            UpdatePreviouslyVisibleRows(isScrollingDown);

            // Try to get the position of a middle visible row and add it.
            int position = GetAndAddMiddleVisibleRowPosition();

            // If no middle row is found, try to find one using binary search.
            if (position == -1)
            {
                position = FindAndAddMiddleVisibleRow();
                // If still not found, then no row is visible.
                if (position == -1) return;
            }

            // Find all visible rows above the found row.
            int margin = isScrollingDown ? 0 : SECURITY_MARGIN;
            for (int i = position - 1; i >= 1; i--)
            {
                int rowId = _tableControl.TableData.Rows[i].Id;
                var header = _tableControl.RowHeaders[rowId];
                if (header.IsVisible || IsHeaderVisible(header) || margin-- > 0)
                    MakeHeaderVisible(header, insertAtTop: true);
                else
                    break;
            }

            // Find all visible rows below the found row.
            margin = isScrollingDown ? SECURITY_MARGIN : 0;
            for (int i = position + 1; i <= _tableControl.RowHeaders.Count; i++)
            {
                int rowId = _tableControl.TableData.Rows[i].Id;
                var header = _tableControl.RowHeaders[rowId];
                if (header.IsVisible || IsHeaderVisible(header) || margin-- > 0)
                    MakeHeaderVisible(header, insertAtTop: false);
                else
                    break;
            }
        }

        /// <summary>
        /// Updates the visibility of rows that were previously visible, depending on the scroll direction.
        /// </summary>
        private void UpdatePreviouslyVisibleRows(bool isScrollingDown)
        {
            if (isScrollingDown)
            {
                /*
                 * As we are scrolling dow, we can assume that from the first visible row we find,
                 * all the following rows are visible. So when we reach that point, we can stop checking.
                 */
                
                bool firstVisibleFound = false;
                foreach (var header in _visibleHeaders)
                {
                    if (!firstVisibleFound)
                    {
                        header.IsVisible = IsHeaderVisible(header);
                        firstVisibleFound = header.IsVisible;
                        if (!firstVisibleFound)
                            NotifyHeaderBecameInvisible(header);
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
                for (int i = _visibleHeaders.Count - 1; i >= 0; i--)
                {
                    var header = _visibleHeaders[i];
                    if (!lastVisibleFound)
                    {
                        header.IsVisible = IsHeaderVisible(header);
                        lastVisibleFound = header.IsVisible;
                        if (!lastVisibleFound)
                            NotifyHeaderBecameInvisible(header);
                    }
                    else
                    {
                        header.IsVisible = true;
                    }
                }
            }
            _visibleHeaders.Clear();
        }

        /// <summary>
        /// Returns the position of the middle row if it is still visible and adds it; otherwise, returns -1.
        /// </summary>
        private int GetAndAddMiddleVisibleRowPosition()
        {
            if (_visibleHeaders.Count > 0 && IsHeaderVisible(_visibleHeaders[_visibleHeaders.Count / 2]))
            {
                int position = _tableControl.RowData[_visibleHeaders[_visibleHeaders.Count / 2].Id].Position;
                int rowId = _tableControl.TableData.Rows[position].Id;
                var midHeader = _tableControl.RowHeaders[rowId];
                MakeHeaderVisible(midHeader, insertAtTop: false);
                return position;
            }
            return -1;
        }

        /// <summary>
        /// Searches for a visible row using binary search and adds it to the list of visible rows.
        /// Returns the position of the found row or -1 if none is found.
        /// </summary>
        private int FindAndAddMiddleVisibleRow()
        {
            int low = 1; // Row positions are 1-based.
            int high = _tableControl.RowHeaders.Count;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int rowId = _tableControl.TableData.Rows[mid].Id;
                var midHeader = _tableControl.RowHeaders[rowId];

                if (midHeader.IsVisible || IsHeaderVisible(midHeader))
                {
                    MakeHeaderVisible(midHeader, insertAtTop: false);
                    return mid;
                }

                if (midHeader.worldBound.yMax < _scrollView.worldBound.yMin)
                {
                    // The 'mid' row is above the visible area; search further down.
                    low = mid + 1;
                }
                else
                {
                    // The 'mid' row is below the visible area; search further up.
                    high = mid - 1;
                }
            }
            return -1;
        }

        protected override bool IsHeaderVisible(RowHeaderControl header)
        {
            var viewBounds = _scrollView.worldBound;

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
    }
}