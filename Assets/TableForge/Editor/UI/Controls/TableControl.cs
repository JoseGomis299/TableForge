using System;
using System.Collections.Generic;
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
        private readonly ScrollView _scrollView;

        private float _scrollViewHeight;
        private float _scrollViewWidth;
        
        private int _page = 0;
        private int _pageNumber = 0;
        
        public IReadOnlyDictionary<int, CellAnchorData> ColumnData => _columnData;
        public IReadOnlyDictionary<int, CellAnchorData> RowData => _rowData;
        public IReadOnlyDictionary<int, RowHeaderControl> RowHeaders => _rowHeaders;
        public IReadOnlyDictionary<int, ColumnHeaderControl> ColumnHeaders => _columnHeaders;
        
        public VisualElement Root { get; }
        public CornerContainerControl CornerContainer => _cornerContainer;
        
        public Table TableData { get; private set; }
        public TableAttributes TableAttributes { get; }
        public BorderResizer HorizontalResizer { get; }
        public BorderResizer VerticalResizer { get; }
        public ICellSelector CellSelector { get; }
        public VisibilityManager<ColumnHeaderControl> ColumnVisibilityManager { get; }
        public VisibilityManager<RowHeaderControl> RowVisibilityManager { get; }
        
        
        public TableControl(VisualElement root)
        {
            // Basic initialization
            Root = root;
            AddToClassList(USSClasses.Table);
            
            TableAttributes = new TableAttributes()
            {
                TableType = TableType.Dynamic,
                ColumnReorderMode = TableReorderMode.Instant,
                RowReorderMode = TableReorderMode.Instant,
                ColumnHeaderVisibility = TableHeaderVisibility.ShowHeaderLetterAndName,
                RowHeaderVisibility = TableHeaderVisibility.ShowHeaderNumberAndName,
            };
            
            // Initialize main components
            _scrollView = CreateScrollView();
            HorizontalResizer = new HorizontalBorderResizer(this);
            VerticalResizer = new VerticalBorderResizer(this);
            ColumnVisibilityManager = new ColumnVisibilityManager(this, _scrollView);
            RowVisibilityManager = new RowVisibilityManager(this, _scrollView);
            CellSelector = new CellSelector(this);

            // Initialize sub-containers
            _rowsContainer = CreateRowsContainer();
            _columnHeaderContainer = new ColumnHeaderContainerControl(_scrollView);
            _rowHeaderContainer = new RowHeaderContainerControl(_scrollView);
            _cornerContainer = new CornerContainerControl(_scrollView);

            // Build UI hierarchy (styles defined in USS)
            BuildLayoutHierarchy();
        }
        
        public TableControl(VisualElement root, TableAttributes attributes)
        {
            // Basic initialization
            Root = root;
            AddToClassList(USSClasses.Table);
            
            TableAttributes = attributes;

            // Initialize main components
            _scrollView = CreateScrollView();
            HorizontalResizer = new HorizontalBorderResizer(this);
            VerticalResizer = new VerticalBorderResizer(this);
            ColumnVisibilityManager = new ColumnVisibilityManager(this, _scrollView);
            RowVisibilityManager = new RowVisibilityManager(this, _scrollView);
            CellSelector = new CellSelector(this);

            // Initialize sub-containers
            _rowsContainer = CreateRowsContainer();
            _columnHeaderContainer = new ColumnHeaderContainerControl(_scrollView);
            _rowHeaderContainer = new RowHeaderContainerControl(_scrollView);
            _cornerContainer = new CornerContainerControl(_scrollView);

            // Build UI hierarchy (styles defined in USS)
            BuildLayoutHierarchy();
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
            _scrollView.Add(scrollviewContentContainer);
            
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
            
            // Add empty data for the corner cell
            _rowData.Add(0, new CellAnchorData(null));
            _columnData.Add(0, new CellAnchorData(null));
            
            _pageNumber = table.Rows.Count > 0 ? table.Rows.Count / ToolbarData.PageSize + 1 : 0;
            _page = table.Rows.Count > 0 ? 1 : 0;
            
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
            foreach (var columnEntry in TableData.Columns)
            {
                var column = columnEntry.Value;
                _columnData.Add(column.Id, new CellAnchorData(column));

                var headerCell = new ColumnHeaderControl(column, this);
                _columnHeaderContainer.Add(headerCell);
                _columnHeaders.Add(column.Id, headerCell);
            }
        }

        private void BuildRows()
        {
            IReadOnlyList<Row> rows = TableData.OrderedRows;
            if(rows.Count == 0)
                return;
            
            for (int i = (_page - 1) * ToolbarData.PageSize; i < rows.Count && i < _page * ToolbarData.PageSize; i++)
            {
                var row = rows[i];
                _rowData.Add(row.Id, new CellAnchorData(row));

                var rowControl = new RowControl(row, this);
                _rowsContainer.Add(rowControl);
                
                var header = new RowHeaderControl(row, this, rowControl);
                _rowHeaders.Add(row.Id, header);
                _rowHeaderContainer.Add(header);
            }
        }
        
        private void RefreshScrollViewHeight(float delta)
        {
            _scrollView.contentContainer.RegisterSingleUseCallback<GeometryChangedEvent>(() => OnScrollviewHeightChanged?.Invoke());

            _scrollViewHeight += delta;
            _scrollView.contentContainer.style.height = _scrollViewHeight;
        }
        
        private void RefreshScrollViewWidth(float delta)
        {
            _scrollView.contentContainer.RegisterSingleUseCallback<GeometryChangedEvent>(() => OnScrollviewWidthChanged?.Invoke());

            _scrollViewWidth += delta;
            _scrollView.contentContainer.style.width = _scrollViewWidth;
        }

        public void RefreshPage()
        {
            _rowsContainer.Clear();
            _rowHeaderContainer.Clear();
            
            BuildRows();
        }
    }
    
    internal class TablePageManager
    {
        private readonly TableControl _tableControl;
        private int _page = 0;
        private readonly int _pageNumber;
        private readonly int _recommendedPageSize;
        
        public int Page => _page;
        public int PageNumber => _pageNumber;
        
        public TablePageManager(TableControl tableControl)
        {
            _tableControl = tableControl;
            
            _pageNumber = tableControl.TableData.Rows.Count > 0 ? tableControl.TableData.Rows.Count / ToolbarData.PageSize + 1 : 0;
            _page = tableControl.TableData.Rows.Count > 0 ? 1 : 0;
            
            
        }
        
        public void NextPage()
        {
            if(_page == _pageNumber)
                return;
            
            _page++;
            _tableControl.RefreshPage();
        }
        
        public void PreviousPage()
        {
            if(_page == 1)
                return;
            
            _page--;
            _tableControl.RefreshPage();
        }
    }
}
