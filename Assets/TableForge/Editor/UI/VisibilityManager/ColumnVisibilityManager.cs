using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        private const float SQUARE_HORIZONTAL_STEP = UiConstants.MinCellWidth * UiConstants.MinCellWidth;

        public ColumnVisibilityManager(TableControl tableControl, ScrollView scrollView) : base(tableControl, scrollView)
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
            if(TableControl.TableData.Columns.Count == 0) return;
            int direction = delta > 0 ? 1 : -1;
            
            // Update visibility of columns that were previously visible.
            foreach (var header in VisibleHeaders)
            {
                bool wasVisible = header.IsVisible;
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible && wasVisible)
                    MakeHeaderInvisible(header);
            }

            VisibleHeaders.Clear();
            
            var orderedColumnHeaders = direction == 1 ?
                TableControl.ColumnHeaders.Values.OrderBy(x => x.CellAnchor.Position) : 
                TableControl.ColumnHeaders.Values.OrderByDescending(x => x.CellAnchor.Position);

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

        public override bool IsHeaderInBounds(ColumnHeaderControl header)
        {
            var viewBounds = ScrollView.contentViewport.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x + SecurityExtraSize.x - TableControl.CornerContainer.worldBound.width, viewBounds.size.y);
            viewBounds.x += TableControl.CornerContainer.worldBound.width;
            
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
    }
}