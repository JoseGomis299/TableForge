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
            _scrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            _tableControl.OnScrollviewWidthChanged += () => RefreshVisibility(_scrollView.horizontalScroller.value);
            _tableControl.RegisterCallback<GeometryChangedEvent>(_ => RefreshVisibility(_scrollView.horizontalScroller.value));
            _tableControl.HorizontalResizer.OnResize += _ => OnHorizontalScroll(_scrollView.horizontalScroller.value);
        }

        public override void Clear()
        {
            base.Clear();
            _lastHorizontalScroll = float.MinValue;
        }

        public override void RefreshVisibility(float value)
        {
            if(_tableControl.TableData.Columns.Count == 0) return;
            bool isScrollingRight = value > 0;

            // Update visibility of columns that were previously visible.
            foreach (var header in _visibleHeaders)
            {
                header.IsVisible = IsHeaderVisible(header);
                if (!header.IsVisible)
                    NotifyHeaderBecameInvisible(header);
            }

            _visibleHeaders.Clear();
            int margin = isScrollingRight ? SECURITY_MARGIN : 0;

            // Loop through all column headers.
            foreach (var header in _tableControl.ColumnHeaders.Values)
            {
                if (IsHeaderVisible(header) || margin-- > 0)
                {
                    MakeHeaderVisible(header, insertAtTop: false);
                }
            }
        }

        private void OnHorizontalScroll(float value)
        {
            float delta = value - _lastHorizontalScroll;
            if (delta * delta < SQUARE_HORIZONTAL_STEP)
                return;

            _lastHorizontalScroll = value;
            RefreshVisibility(value);
        }

        protected override bool IsHeaderVisible(ColumnHeaderControl header)
        {
            if(_lockedVisibleHeaders.Contains(header)) return true;
            var viewBounds = _scrollView.worldBound;

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