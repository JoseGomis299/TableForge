using System;
using System.Collections.Generic;
using System.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableControl : VisualElement
    {
        public event Action OnScrollviewHeightChanged;
        public event Action OnScrollviewWidthChanged;
        
        private readonly Dictionary<int, CellAnchorData> _columnData = new Dictionary<int, CellAnchorData>();
        private readonly Dictionary<int, CellAnchorData> _rowData = new Dictionary<int, CellAnchorData>();
        private readonly Dictionary<int, RowHeaderControl> _rowHeaders = new Dictionary<int, RowHeaderControl>();
        private readonly Dictionary<int, ColumnHeaderControl> _columnHeaders = new Dictionary<int, ColumnHeaderControl>();
        
        private readonly ColumnHeaderContainerControl _columnHeaderContainer;
        private readonly RowHeaderContainerControl _rowHeaderContainer;
        private readonly CornerContainerControl _cornerContainer;
        private readonly VisualElement _rowsContainer;

        private float _scrollViewHeight;
        private float _scrollViewWidth;
        
        
        public IReadOnlyDictionary<int, CellAnchorData> ColumnData => _columnData;
        public IReadOnlyDictionary<int, CellAnchorData> RowData => _rowData;
        public IReadOnlyDictionary<int, RowHeaderControl> RowHeaders => _rowHeaders;
        public IReadOnlyDictionary<int, ColumnHeaderControl> ColumnHeaders => _columnHeaders;
        
        public VisualElement Root { get; }
        public CornerContainerControl CornerContainer => _cornerContainer;

        public Table TableData { get; private set; }
        public ScrollView ScrollView { get; }
        public TablePageManager PageManager { get; private set; }
        public TableAttributes TableAttributes { get; private set; }
        public BorderResizer HorizontalResizer { get; }
        public BorderResizer VerticalResizer { get; }
        public ICellSelector CellSelector { get; }
        public HeaderSwapper HeaderSwapper { get; }
        public SubTableCellControl Parent { get; }
        public VisibilityManager<ColumnHeaderControl> ColumnVisibilityManager { get; }
        public VisibilityManager<RowHeaderControl> RowVisibilityManager { get; }
        public bool[] VisibleColumns { get; private set; }
        public bool Inverted { get; private set; }
        
        public TableControl(VisualElement root, TableAttributes attributes, SubTableCellControl parent)
        {
            // Basic initialization
            Root = root;
            AddToClassList(USSClasses.Table);
            TableAttributes = attributes;
            Parent = parent;

            // Initialize main components
            ScrollView = CreateScrollView();
            HorizontalResizer = new HorizontalBorderResizer(this);
            VerticalResizer = new VerticalBorderResizer(this);
            ColumnVisibilityManager = new ColumnVisibilityManager(this, ScrollView);
            RowVisibilityManager = new RowVisibilityManager(this, ScrollView);
            CellSelector = new CellSelector(this);
            HeaderSwapper = new HeaderSwapper(this);

            // Initialize sub-containers
            _rowsContainer = CreateRowsContainer();
            _columnHeaderContainer = new ColumnHeaderContainerControl(ScrollView);
            _rowHeaderContainer = new RowHeaderContainerControl(ScrollView);
            _cornerContainer = new CornerContainerControl(ScrollView);

            // Build UI hierarchy (styles defined in USS)
            BuildLayoutHierarchy();
        }

        public void Update()
        {
            foreach (var rowHeader in RowVisibilityManager.VisibleHeaders)
            {
                foreach (var columnHeader in ColumnVisibilityManager.VisibleHeaders)
                { 
                    var cell = GetCell(rowHeader.Id, columnHeader.Id);
                    cell?.Refresh();
                }
            }
        }
        
        public void UpdateRow(int rowId)
        {
            if (!_rowHeaders.ContainsKey(rowId))
                return;

            foreach (var columnId in _columnHeaders.Keys)
            {
                var cell = GetCell(rowId, columnId);
                cell?.Refresh();
            }
        }

        public void Invert()
        {
            Inverted = !Inverted;

            TableAttributes reversedAttributes = new TableAttributes
            {
                ColumnHeaderVisibility = TableAttributes.RowHeaderVisibility,
                RowHeaderVisibility = TableAttributes.ColumnHeaderVisibility,
                RowReorderMode = TableAttributes.ColumnReorderMode,
                ColumnReorderMode = TableAttributes.RowReorderMode,
                TableType = TableAttributes.TableType
            };  
 
            TableAttributes = reversedAttributes;
        }
        
        private ScrollView CreateScrollView()
        {
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.AddToClassList(USSClasses.Fill);
            Add(scrollView);
            
            return scrollView;
        }
        
        private VisualElement CreateRowsContainer()
        {
            var container = new VisualElement();
            container.AddToClassList(USSClasses.TableRowContainer);
            return container;
        }
        
        private void BuildLayoutHierarchy()
        {
            // Container for all scroll view content
            var scrollviewContentContainer = new VisualElement();
            scrollviewContentContainer.AddToClassList(USSClasses.TableScrollViewContentContainer);
            ScrollView.Add(scrollviewContentContainer);
            
            /*
             Note that the bottom container is added first, as the scrollviewContentContainer flex-direction is columnReverse.
             This is done to ensure that the bottom container is rendered first and the top container is rendered last
             while still maintaining the correct order
            */
             
            // Bottom container for rows and row headers
            var bottomContainer = new VisualElement();
            bottomContainer.AddToClassList(USSClasses.TableScrollViewContentBottom);
            scrollviewContentContainer.Add(bottomContainer);
            bottomContainer.Add(_rowsContainer);
            bottomContainer.Add(_rowHeaderContainer);
            
            // Top container for column headers and corner cell
            var topContainer = new VisualElement();
            topContainer.AddToClassList(USSClasses.TableScrollViewContentTop);
            scrollviewContentContainer.Add(topContainer);
            var cornerCell = new TableCornerControl(this, _columnHeaderContainer, _rowHeaderContainer, _rowsContainer);
            _cornerContainer.Add(cornerCell);
            topContainer.Add(_columnHeaderContainer);
            topContainer.Add(_cornerContainer);
        }
        
        public void SetTable(Table table)
        {
            TableData = table;
            _scrollViewHeight = UiConstants.CellHeight; //This is the column header height

            _columnData.Clear();
            _rowData.Clear();
            _rowHeaders.Clear();
            _columnHeaders.Clear();
            _columnHeaderContainer.Clear();
            _rowsContainer.Clear();
            
            PageManager = new TablePageManager(this);
            PageManager.RecalculatePage();
            
            VisibleColumns = new bool[TableData.Columns.Count];
            for (int i = 0; i < VisibleColumns.Length; i++)
                VisibleColumns[i] = true;
            

            // Add empty data for the corner cell
            _rowData.Add(0, new CellAnchorData(null));
            _columnData.Add(0, new CellAnchorData(null));
            
            BuildHeader();
            BuildRows();
            
            this.RegisterSingleUseCallback<GeometryChangedEvent>(OnGeometryInitialized);
        }
        
        private void OnGeometryInitialized()
        {
            VerticalResizer.OnResize += RefreshScrollViewHeight;
            HorizontalResizer.OnResize += RefreshScrollViewWidth;

            HorizontalResizer.ResizeAll();
            VerticalResizer.ResizeAll();
        }
        
        private void BuildHeader()
        {

            if (!Inverted)
            {
                foreach (var column in TableData.OrderedColumns)
                {
                    _columnData.Add(column.Id, new CellAnchorData(column));

                    var headerCell = new ColumnHeaderControl(column, this);
                    _columnHeaderContainer.Add(headerCell);
                    _columnHeaders.Add(column.Id, headerCell);
                }
            }
            else
            {
                IReadOnlyList<Row> rows = TableData.OrderedRows;
                for (int i = PageManager.FirstRowPosition - 1; i < rows.Count && i < PageManager.LastRowPosition; i++)
                {
                    var row = rows[i];
                    _columnData.Add(row.Id, new CellAnchorData(row));

                    var headerCell = new ColumnHeaderControl(row, this);
                    _columnHeaderContainer.Add(headerCell);
                    _columnHeaders.Add(row.Id, headerCell);
                }
            }
            
        }

        private void BuildRows()
        {
            IReadOnlyList<Row> rows = TableData.OrderedRows;
            if(rows.Count == 0)
                return;

            if (!Inverted)
            {
                for (int i = PageManager.FirstRowPosition - 1; i < rows.Count && i < PageManager.LastRowPosition; i++)
                {
                    var row = rows[i];
                    _rowData.Add(row.Id, new CellAnchorData(row));

                    var rowControl = new RowControl(row, this);
                    _rowsContainer.Add(rowControl);

                    var header = new RowHeaderControl(row, this, rowControl);
                    _rowHeaders.Add(row.Id, header);
                    _rowHeaderContainer.Add(header);
                    HeaderSwapper.HandleSwapping(header);
                }
            }
            else
            {
                foreach (var column in TableData.OrderedColumns)
                {
                    _rowData.Add(column.Id, new CellAnchorData(column));

                    var rowControl = new RowControl(column, this);
                    _rowsContainer.Add(rowControl);

                    var header = new RowHeaderControl(column, this, rowControl);
                    _rowHeaders.Add(column.Id, header);
                    _rowHeaderContainer.Add(header);
                    HeaderSwapper.HandleSwapping(header);
                }
            }
        }
        
        private void RefreshScrollViewHeight(float delta)
        {
            ScrollView.contentContainer.RegisterCallbackOnce<GeometryChangedEvent>(_ => OnScrollviewHeightChanged?.Invoke());

            _scrollViewHeight += delta;
            ScrollView.verticalScroller.value = Mathf.Min(_scrollViewHeight, ScrollView.verticalScroller.value);
            ScrollView.contentContainer.style.height = _scrollViewHeight;
        }
        
        private void RefreshScrollViewWidth(float delta)
        {
            ScrollView.contentContainer.RegisterCallbackOnce<GeometryChangedEvent>(_ => OnScrollviewWidthChanged?.Invoke());

            _scrollViewWidth += delta;
            ScrollView.horizontalScroller.value = Mathf.Min(_scrollViewWidth, ScrollView.horizontalScroller.value);
            ScrollView.contentContainer.style.width = _scrollViewWidth;
        }

        public CellControl GetCell(int rowId, int columnId)
        {
            if (!_rowData.ContainsKey(rowId) || !_columnData.ContainsKey(columnId))
                return null;

            int rowIndex = _rowData[rowId].Position - PageManager.FirstRowPosition;
            int columnIndex = _columnData[columnId].Position - 1;

            if (rowIndex < 0 || columnIndex < 0)
                return null;
            
            return _rowsContainer[rowIndex].ElementAt(columnIndex) as CellControl;
        }

        public void RebuildPage()
        {
            _scrollViewHeight = UiConstants.CellHeight; //This is the column header height
            _scrollViewWidth = 0;
            
            CellSelector.ClearSelection();

            foreach (var rowHeader in _rowHeaderContainer.Children())
            {
                if (rowHeader is RowHeaderControl rowHeaderControl)
                {
                    VerticalResizer.Dispose(rowHeaderControl);
                    HeaderSwapper.Dispose(rowHeaderControl);
                }
            }
            _rowHeaders.Clear();
            _rowHeaderContainer.Clear();
            _rowData.Clear();
            _rowsContainer.Clear();

            foreach (var columnHeader in _columnHeaderContainer.Children())
            {
                if(columnHeader is ColumnHeaderControl columnHeaderControl)
                    HorizontalResizer.Dispose(columnHeaderControl);
            }
            _columnHeaderContainer.Clear();
            _columnData.Clear();
            _columnHeaders.Clear();
            
            RowVisibilityManager.Clear();
            ColumnVisibilityManager.Clear();
            PageManager.RecalculatePage();
                        
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            // Add empty data for the corner cell
            _rowData.Add(0, new CellAnchorData(null));
            _columnData.Add(0, new CellAnchorData(null));
            
            BuildHeader();
            BuildRows();

            _cornerContainer.CornerControl.style.width = 0;
            HorizontalResizer.ResizeAll();
            VerticalResizer.ResizeAll();
        }
        
        public void RefreshPage()
        {
            foreach (var row in _rowHeaders.Values)
            {
                row.Refresh();
            }
        }
        
        public void ShowScrollbars(bool value)
        {
            ScrollView.horizontalScrollerVisibility = value ? ScrollerVisibility.Auto : ScrollerVisibility.Hidden;
            ScrollView.verticalScrollerVisibility = value ? ScrollerVisibility.Auto : ScrollerVisibility.Hidden;
        }

        public void MoveRow(int rowStartPos, int rowEndPos)
        {
            if (rowStartPos == rowEndPos)
                return;

            if (TableAttributes.RowReorderMode == TableReorderMode.ExplicitReorder)
            {
                int startIndex = rowStartPos - 1;
                int endIndex = rowEndPos - 1;
                bool isMovingUp = rowStartPos > rowEndPos;
                int currentIndex = startIndex;
                while (currentIndex != endIndex)
                {
                    var nextIndex = isMovingUp ? currentIndex - 1 : currentIndex + 1;

                    _rowsContainer.SwapChildren(currentIndex, nextIndex);
                    _rowHeaderContainer.SwapChildren(currentIndex, nextIndex);

                    currentIndex = nextIndex;
                }
            }

            CellSelector.ClearSelection();
            TableData.MoveRow(rowStartPos, rowEndPos);
            RefreshPage();
        }
        
        public CellAnchor GetRowAtPosition(int position)
        {
            if (!Inverted)
            {
                if (TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }
            else
            {
                if (TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }

            return null;
        }
        
        public CellAnchor GetColumnAtPosition(int position)
        {
            if (!Inverted)
            {
                if (TableData.Columns.TryGetValue(position, out var column))
                    return column;
            }
            else
            {
                if (TableData.Rows.TryGetValue(position, out var row))
                    return row;
            }

            return null;
        }

        public CellAnchor GetCellRow(CellControl cell)
        {
            return !Inverted ? cell.Cell.Row : cell.Cell.Column;
        }
        
        public CellAnchor GetCellColumn(CellControl cell)
        {
            return !Inverted ? cell.Cell.Column : cell.Cell.Row;
        }

        public int GetColumnPosition(int columnId)
        {
            if (!_columnData.ContainsKey(columnId))
                return -1;

            return Inverted ? _columnData[columnId].Position - PageManager.FirstRowPosition + 1 : _columnData[columnId].Position;
        }
    }
}
