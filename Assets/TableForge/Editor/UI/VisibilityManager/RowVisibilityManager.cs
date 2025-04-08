using System.Linq;
using UnityEngine;

namespace TableForge.UI
{
    internal class RowVisibilityManager : VisibilityManager<RowHeaderControl>
    {
        private const float SQUARE_VERTICAL_STEP = UiConstants.MinCellHeight * UiConstants.MinCellHeight;
        
        public RowVisibilityManager(TableControl tableControl) : base(tableControl)
        {
            ScrollView.verticalScroller.valueChanged += OnVerticalScroll;
            TableControl.OnScrollviewSizeChanged += delta =>
            {
                if(delta.y == 0 && delta.x != 0) return;

                RefreshVisibility(delta.y);
            };
        }

        private void OnVerticalScroll(float value)
        {
            float delta = value - LastScrollValue;
            if (delta * delta < SQUARE_VERTICAL_STEP)
                return;

            LastScrollValue = value;
            RefreshVisibility(delta);
        }

//         protected override void NotifyHeaderBecameVisible(RowHeaderControl header, int direction)
//         {
//             Debug.Log($"Row {header.Name} became visible");
//             base.NotifyHeaderBecameVisible(header, direction);
//         }
//
//         protected override void NotifyHeaderBecameInvisible(RowHeaderControl header, int direction)
//         {
//             Debug.Log($"Row {header.Name} became invisible");
//             base.NotifyHeaderBecameInvisible(header, direction);
//         }

        public override void RefreshVisibility(float delta)
        {
            if(TableControl.RowData.Count <= 1) return;
            
            int direction = delta > 0 ? -1 : 1;
            bool isScrollingDown = direction == -1;

            int startingIndex = TableControl.RowData.Values.OrderBy(x => x.Position).First(x => x.Position > 0).Position;
            int endingIndex = startingIndex + TableControl.RowData.Count - 2;

            // Update visibility of rows that were previously visible based on scroll direction.
            UpdatePreviouslyVisibleRows(isScrollingDown);

            // Try to get the position of a middle visible row and add it.
            int position = GetAndAddMiddleVisibleRowPosition();

            // If no middle row is found, try to find one using binary search.
            if (position == -1)
            {
                position = FindAndAddMiddleVisibleRow(startingIndex, endingIndex);
                // If still not found, then no row is visible.
                if (position == -1)
                {
                    SendVisibilityNotifications(direction);
                    return;
                }
            }

            // Find all visible rows above the found row.
            for (int i = position - 1; i >= startingIndex ; i--)
            {
                int rowId = TableControl.GetRowAtPosition(i).Id;
                var header = TableControl.RowHeaders[rowId];
                if (header.IsVisible || IsHeaderVisible(header))
                    MakeHeaderVisible(header, insertAtTop: true);
                else
                    break;
            }

            // Find all visible rows below the found row.
            for (int i = position + 1; i <= endingIndex; i++)
            {
                int rowId = TableControl.GetRowAtPosition(i).Id;
                var header = TableControl.RowHeaders[rowId];
                if (header.IsVisible || IsHeaderVisible(header))
                    MakeHeaderVisible(header, insertAtTop: false);
                else
                    break;
            }
            
            SendVisibilityNotifications(direction);
        }

        /// <summary>
        /// Updates the visibility of rows that were previously visible, depending on the scroll direction.
        /// </summary>
        private void UpdatePreviouslyVisibleRows(bool isScrollingDown)
        {
            if (isScrollingDown)
            {
                /*
                 * As we are scrolling down, we can assume that from the first visible row we find,
                 * all the following rows are visible. So when we reach that point, we can stop checking.
                 */
                
                bool firstVisibleFound = false;
                foreach (var header in VisibleHeaders)
                {
                    if (!firstVisibleFound)
                    {
                        bool wasVisible = header.IsVisible;
                        header.IsVisible = IsHeaderVisible(header);
                        firstVisibleFound = header.IsVisible;
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
                for (int i = VisibleHeaders.Count - 1; i >= 0; i--)
                {
                    var header = VisibleHeaders[i];
                    if (!lastVisibleFound)
                    {
                        bool wasVisible = header.IsVisible;
                        header.IsVisible = IsHeaderVisible(header);
                        lastVisibleFound = header.IsVisible;
                        if (!lastVisibleFound && wasVisible)
                            MakeHeaderInvisible(header);
                    }
                    else
                    {
                        header.IsVisible = true;
                    }
                }
            }
            VisibleHeaders.Clear();
        }

        /// <summary>
        /// Returns the position of the middle row if it is still visible and adds it; otherwise, returns -1.
        /// </summary>
        private int GetAndAddMiddleVisibleRowPosition()
        {
            if (VisibleHeaders.Count > 0 && IsHeaderVisible(VisibleHeaders[VisibleHeaders.Count / 2]))
            {
                int position = TableControl.RowData[VisibleHeaders[VisibleHeaders.Count / 2].Id].Position;
                int rowId = TableControl.GetRowAtPosition(position).Id;
                var midHeader = TableControl.RowHeaders[rowId];
                MakeHeaderVisible(midHeader, insertAtTop: false);
                return position;
            }
            return -1;
        }

        /// <summary>
        /// Searches for a visible row using binary search and adds it to the list of visible rows.
        /// Returns the position of the found row or -1 if none is found.
        /// </summary>
        private int FindAndAddMiddleVisibleRow(int startingIndex, int endingIndex)
        {
            int low = startingIndex; // Row positions are 1-based.
            int high = endingIndex;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int rowId = TableControl.GetRowAtPosition(mid).Id;
                var midHeader = TableControl.RowHeaders[rowId];

                if (midHeader.IsVisible || IsHeaderVisible(midHeader))
                {
                    MakeHeaderVisible(midHeader, insertAtTop: false);
                    return mid;
                }

                if (midHeader.worldBound.yMax < ScrollView.worldBound.yMin)
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

        public override bool IsHeaderInBounds(RowHeaderControl header)
        {
            var viewBounds = ScrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.width, viewBounds.height + SecurityExtraSize.y);

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