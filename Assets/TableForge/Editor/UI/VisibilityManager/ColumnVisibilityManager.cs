using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        private const float SQUARE_HORIZONTAL_STEP = UiConstants.MinCellWidth * UiConstants.MinCellWidth;

        private readonly TableControl _tableControl;
        private float _lastHorizontalScroll;

        public ColumnVisibilityManager(TableControl tableControl, ScrollView scrollView)
            : base(scrollView)
        {
            _tableControl = tableControl;
            _lastHorizontalScroll = float.MinValue;

            // Subscribe to horizontal scroll changes.
            ScrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            _tableControl.OnScrollviewWidthChanged += () => RefreshVisibility(0);
            
            LastDirection = 1;
        }

        public override void Clear()
        {
            base.Clear();
            _lastHorizontalScroll = float.MinValue;
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
            if(_tableControl.TableData.Columns.Count == 0) return;
            
            // Update visibility of columns that were previously visible.
            foreach (var header in VisibleHeaders)
            {
                bool wasVisible = header.IsVisible;
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible && wasVisible)
                    NotifyHeaderBecameInvisible(header, LastDirection);
            }

            VisibleHeaders.Clear();

            var orderedColumnHeaders = LastDirection == 1 ?
                _tableControl.ColumnHeaders.Values.OrderBy(x => x.CellAnchor.Position) : 
                _tableControl.ColumnHeaders.Values.OrderByDescending(x => x.CellAnchor.Position);

            // Loop through all column headers.
            foreach (var header in orderedColumnHeaders)
            {
                if (header.IsVisible || IsHeaderVisible(header))
                {
                    MakeHeaderVisible(header, insertAtTop: false, LastDirection);
                }
            }
        }

        private void OnHorizontalScroll(float value)
        {
            float delta = value - _lastHorizontalScroll;
            if (delta * delta < SQUARE_HORIZONTAL_STEP)
                return;

            _lastHorizontalScroll = value;
            LastDirection = delta > 0 ? 1 : -1;
            RefreshVisibility(delta);
        }

        protected override bool IsHeaderVisible(ColumnHeaderControl header)
        {
            if(LockedVisibleHeaders.Contains(header)) return true;
            var viewBounds = ScrollView.worldBound.width <= 1 ? ScrollView.contentContainer.worldBound : ScrollView.worldBound;
            viewBounds.size = new Vector2(viewBounds.size.x + SecurityExtraSize.x, viewBounds.size.y);

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