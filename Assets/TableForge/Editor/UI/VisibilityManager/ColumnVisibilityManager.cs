using System.Linq;
using UnityEngine;

namespace TableForge.UI
{
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        private const float SQUARE_HORIZONTAL_STEP = UiConstants.MinCellWidth * UiConstants.MinCellWidth;

        public ColumnVisibilityManager(TableControl tableControl) : base(tableControl)
        {
            ScrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            TableControl.OnScrollviewSizeChanged += delta =>
            {
                if(delta.x == 0 && delta.y != 0) return;
                
                RefreshVisibility(1);
            };
            
        }
        
        // protected override void NotifyHeaderBecameVisible(ColumnHeaderControl header, int direction)
        // {
        //     Debug.Log($"Column {header.Name} became visible");
        //     base.NotifyHeaderBecameVisible(header, direction);
        // }
        //
        // protected override void NotifyHeaderBecameInvisible(ColumnHeaderControl header, int direction)
        // {
        //     Debug.Log($"Column {header.Name} became invisible");
        //     base.NotifyHeaderBecameInvisible(header, direction);
        // }

        public override void RefreshVisibility(float delta)
        {
            if(TableControl.TableData.Columns.Count == 0
               || TableControl.Parent is ExpandableSubTableCellControl { IsFoldoutOpen: false })
                return;
            int direction = delta > 0 ? 1 : -1;
            
            // Update visibility of columns that were previously visible.
            foreach (var header in VisibleHeaders)
            {
                bool wasVisible = header.IsVisible && !LockedVisibleHeaders.ContainsKey(header);
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible && wasVisible)
                    MakeHeaderInvisible(header);
            }

            VisibleHeaders.Clear();
            
            var orderedColumnHeaders = direction == 1 ?
                TableControl.OrderedColumnHeaders : 
                TableControl.OrderedDescColumnHeaders;

            // Loop through all column headers.
            foreach (var header in orderedColumnHeaders)
            {
                if (header.IsVisible || IsHeaderVisible(header))
                {
                    MakeHeaderVisible(header, insertAtTop: false);
                }
            }
            
            SendVisibilityNotifications(direction);
        }

        private void OnHorizontalScroll(float value)
        {
            float delta = value - LastScrollValue;
            if (delta * delta < SQUARE_HORIZONTAL_STEP)
                return;

            LastScrollValue = value;
            RefreshVisibility(delta);
        }

        public override bool IsHeaderInBounds(ColumnHeaderControl header, bool addSecuritySize)
        {
            Vector2 securitySize = addSecuritySize ? new Vector2(SecurityExtraSize.x, 0) : Vector2.zero;
            var viewBounds = ScrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x - TableControl.CornerContainer.worldBound.width, viewBounds.size.y) + securitySize / 2f;
            viewBounds.x += TableControl.CornerContainer.worldBound.width - securitySize.x / 2f;
            
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
            Vector2 securitySize = addSecuritySize ? new Vector2(SecurityExtraSize.x, 0) : Vector2.zero;
            var viewBounds = ScrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x - TableControl.CornerContainer.worldBound.width, viewBounds.size.y) + securitySize / 2f;
            viewBounds.x += TableControl.CornerContainer.worldBound.width - securitySize.x / 2f;

            bool isLeftSideVisible = header.worldBound.xMin <= viewBounds.xMax &&
                                     header.worldBound.xMin >= viewBounds.xMin;

            bool isRightSideVisible = header.worldBound.xMax >= viewBounds.xMin &&
                                      header.worldBound.xMax <= viewBounds.xMax;

            visibleBounds = 0;
            if (isLeftSideVisible) visibleBounds += 1;
            if (isRightSideVisible) visibleBounds += 2;

            return isLeftSideVisible && isRightSideVisible;
        }
    }
}