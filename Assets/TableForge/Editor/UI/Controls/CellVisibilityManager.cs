using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class CellVisibilityManager
    {
        public event Action<HeaderControl> OnHeaderBecameVisible;
        
        private const float SQUARE_VERTICAL_STEP = UiContants.MinCellHeight * UiContants.MinCellHeight;
        private const float SQUARE_HORIZONTAL_STEP = UiContants.MinCellWidth * UiContants.MinCellWidth;
        private const int SECURITY_MARGIN = 2;
        
        private readonly TableControl _tableControl;
        
        private List<RowHeaderControl> _visibleRows = new List<RowHeaderControl>();
        private List<ColumnHeaderControl> _visibleColumns = new List<ColumnHeaderControl>();
        public IReadOnlyList<RowHeaderControl> VisibleRows => _visibleRows;
        public IReadOnlyList<ColumnHeaderControl> VisibleColumns => _visibleColumns;
        
        private float _lastHorizontalScroll;
        private float _lastVerticalScroll;
        
        public CellVisibilityManager(TableControl tableControl)
        {
            _tableControl = tableControl;
            _lastHorizontalScroll = float.MinValue;
            _lastVerticalScroll = float.MinValue;
            
            _tableControl.ScrollView.horizontalScroller.valueChanged += OnHorizontalScroll;
            _tableControl.ScrollView.verticalScroller.valueChanged += OnVerticalScroll;
            
            _tableControl.ScrollView.RegisterCallback<GeometryChangedEvent>(e => Initialize());
        }
        
        public void Initialize()
        {
            OnHorizontalScroll(0);
            OnVerticalScroll(0);
        }
        
        private void OnHorizontalScroll(float value)
        {
            float delta = value - _lastHorizontalScroll;
            if(delta * delta < SQUARE_HORIZONTAL_STEP) return;
            _lastHorizontalScroll = value;
            
            foreach (var column in _visibleColumns)
            {
                column.IsVisible = false;
            }
            
            _visibleColumns.Clear();
            int margin = SECURITY_MARGIN;
            foreach (var header in _tableControl.ColumnHeaders.Values)
            {
                if (IsVisible(header) || margin-- > 0)  
                {
                    _visibleColumns.Add(header);
                    header.IsVisible = true;
                    OnHeaderBecameVisible?.Invoke(header);
                }
            }
            
            bool IsVisible(ColumnHeaderControl header)
            {
                return header.worldBound.xMin < _tableControl.ScrollView.worldBound.xMax 
                       && header.worldBound.xMin > _tableControl.ScrollView.worldBound.xMin;
            }
        }
        
        private void OnVerticalScroll(float value)
        {
            float delta = value - _lastVerticalScroll;
            if(delta * delta < SQUARE_VERTICAL_STEP) return;
            _lastVerticalScroll = value;
            
            int index = -1;

            //Check if the previous shown rows are still visible
            if (_visibleRows.Count > 0 && IsVisible(_visibleRows[_visibleRows.Count / 2]))
            {
                index = _tableControl.RowHeaderContainer.IndexOf(_visibleRows[_visibleRows.Count / 2]);
            }
            
            foreach (var row in _visibleRows)
            {
                row.IsVisible = false;
            }
            _visibleRows.Clear();
            
            //if the previous shown rows are no longer visible find some visible row using binary search
            if (index == -1)
            {
                int low = 0;
                int high = _tableControl.RowHeaderContainer.Children().Count() - 1;
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    RowHeaderControl midHeader = _tableControl.RowHeaderContainer.ElementAt(mid) as RowHeaderControl;
                    if (IsVisible(midHeader))
                    {
                        _visibleRows.Add(midHeader);
                        midHeader.IsVisible = true;
                        OnHeaderBecameVisible?.Invoke(midHeader);

                        index = mid;
                        break;
                    }

                    if (midHeader != null && midHeader.worldBound.yMax < _tableControl.ScrollView.worldBound.yMin)
                    {
                        //mid is above the visible area so we need to search below
                        low = mid + 1;
                    }
                    else
                    {
                        //mid is below the visible area so we need to search above
                        high = mid - 1;
                    }
                }
            }
            else
            {
                RowHeaderControl midHeader = _tableControl.RowHeaderContainer.ElementAt(index) as RowHeaderControl;
                midHeader.IsVisible = true;
                _visibleRows.Add(midHeader);
                OnHeaderBecameVisible?.Invoke(midHeader);
            }

            
            //find all visible rows above the found row
            int margin = SECURITY_MARGIN;
            for (int i = index - 1; i >= 0; i--)
            {
                RowHeaderControl header = _tableControl.RowHeaderContainer.ElementAt(i) as RowHeaderControl;
                if (IsVisible(header) || margin-- > 0)
                {
                    _visibleRows.Insert(0, header);
                    header.IsVisible = true;
                    OnHeaderBecameVisible?.Invoke(header);
                }
                else
                {
                    break;
                }
            }
            
            //find all visible rows below the found row
            margin = SECURITY_MARGIN;
            for (int i = index + 1; i < _tableControl.RowHeaders.Count; i++)
            {
                RowHeaderControl header = _tableControl.RowHeaderContainer.ElementAt(i) as RowHeaderControl;
                if (IsVisible(header) || margin-- > 0)
                {
                    _visibleRows.Add(header);
                    header.IsVisible = true;
                    OnHeaderBecameVisible?.Invoke(header);
                }
                else
                {
                    break;
                }
            }

            bool IsVisible(RowHeaderControl header)
            {
                return header.worldBound.yMax < _tableControl.ScrollView.worldBound.yMax 
                       && header.worldBound.yMax > _tableControl.ScrollView.worldBound.yMin;
            }
        }
    }
}