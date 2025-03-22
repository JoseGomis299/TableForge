using System.Linq;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ColumnVisibilityManager : VisibilityManager<ColumnHeaderControl>
    {
        private const float SQUARE_HORIZONTAL_STEP = UiConstants.MinCellWidth * UiConstants.MinCellWidth;
        private const int SECURITY_MARGIN = 1;

        private readonly TableControl _tableControl;
        private float _lastHorizontalScroll;

        public ColumnVisibilityManager(TableControl tableControl, ScrollView scrollView)
            : base(scrollView)
        {
            _tableControl = tableControl;
            _lastHorizontalScroll = float.MinValue;

            // Subscribe to horizontal scroll changes.
            ScrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            _tableControl.OnScrollviewWidthChanged += () => RefreshVisibility(-1);
        }

        public override void Clear()
        {
            base.Clear();
            _lastHorizontalScroll = float.MinValue;
        }

        public override void RefreshVisibility(float delta)
        {
            if(_tableControl.TableData.Columns.Count == 0) return;
            
            bool isScrollingRight = delta > 0;

            // Update visibility of columns that were previously visible.
            foreach (var header in VisibleHeaders)
            {
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible)
                    NotifyHeaderBecameInvisible(header, LastDirection);
            }

            VisibleHeaders.Clear();
            int margin = isScrollingRight ? SECURITY_MARGIN : 0;

            // Loop through all column headers.
            foreach (var header in _tableControl.ColumnHeaders.Values)
            {
                if (IsHeaderVisible(header) || margin-- > 0)
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
            var viewBounds = ScrollView.worldBound.width == 0 ? ScrollView.contentContainer.worldBound : ScrollView.worldBound;

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