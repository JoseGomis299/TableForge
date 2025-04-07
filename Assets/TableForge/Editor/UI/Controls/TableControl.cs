using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableControl : VisualElement
    {
        public event Action<Vector2> OnScrollviewSizeChanged; 
        
        private readonly Dictionary<int, CellAnchorData> _columnData = new();
        private readonly Dictionary<int, CellAnchorData> _rowData = new();
        private readonly Dictionary<int, RowHeaderControl> _rowHeaders = new();
        private readonly Dictionary<int, ColumnHeaderControl> _columnHeaders = new();
        
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
        public TableSize PreferredSize { get; private set; }
        public TableMetadata Metadata { get; private set; }
        public ScrollView ScrollView { get; }
        public TableAttributes TableAttributes { get; private set; }
        public TableResizer Resizer { get; }
        public BorderResizer HorizontalResizer => Resizer.HorizontalResizer;
        public BorderResizer VerticalResizer => Resizer.VerticalResizer;
        public ICellSelector CellSelector { get; }
        public HeaderSwapper HeaderSwapper { get; }
        public SubTableCellControl Parent { get; }
        public VisibilityManager<ColumnHeaderControl> ColumnVisibilityManager { get; }
        public VisibilityManager<RowHeaderControl> RowVisibilityManager { get; }
        public bool Inverted { get; private set; }
        public float RowsContainerOffset { get; private set; }
        
        public TableControl(VisualElement root, TableAttributes attributes, SubTableCellControl parent)
        {
            // Basic initialization
            Root = root;
            AddToClassList(USSClasses.Table);
            TableAttributes = attributes;
            Parent = parent;

            // Initialize main components
            ScrollView = CreateScrollView();
            Resizer = new TableResizer(this);
            ColumnVisibilityManager = new ColumnVisibilityManager(this, ScrollView);
            RowVisibilityManager = new RowVisibilityManager(this, ScrollView);
            CellSelector = parent != null ? parent.TableControl.CellSelector : new CellSelector(this);
            HeaderSwapper = new HeaderSwapper(this);
            
            // Initialize sub-containers
            _rowsContainer = CreateRowsContainer();
            _columnHeaderContainer = new ColumnHeaderContainerControl(ScrollView);
            _rowHeaderContainer = new RowHeaderContainerControl(ScrollView);
            _cornerContainer = new CornerContainerControl(ScrollView);
            
            // Subscribe to visibility events
            RowVisibilityManager.OnHeaderBecameVisible += OnRowHeaderBecameVisible;
            RowVisibilityManager.OnHeaderBecameInvisible += OnRowHeaderBecameInvisible;
            ColumnVisibilityManager.OnHeaderBecameVisible += OnColumnHeaderBecameVisible;
            ColumnVisibilityManager.OnHeaderBecameInvisible += OnColumnHeaderBecameInvisible;
            
            // Build UI hierarchy (styles defined in USS)
            BuildLayoutHierarchy();
        }
        
        public void SetTable(Table table)
        {
            TableData = table;
            Metadata = Parent == null ? TableMetadataManager.GetMetadata(table, table.Name) : Parent.TableControl.Metadata;
            if(!Inverted && Metadata.IsInverted) Invert();
            
            if(_columnData.Any()) ClearTable();
            PreferredSize = SizeCalculator.CalculateTableSize(table, TableAttributes, Metadata);
            
            // Add empty data for the corner cell
            _rowData.Add(0, new CellAnchorData(null));
            _columnData.Add(0, new CellAnchorData(null));
            
            BuildHeader();
            BuildRows();
            
            InitializeGeometry();
        }

        public void Update()
        {
            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
                foreach (var columnHeader in ColumnVisibilityManager.CurrentVisibleHeaders)
                { 
                    var cell = GetCellControl(rowHeader.Id, columnHeader.Id);
                    cell?.Refresh();
                }
            }
        }
        
        public void UpdateRow(int rowId)
        {
            if (!_rowHeaders.ContainsKey(rowId))
                return;

            foreach (var columnHeader in ColumnVisibilityManager.CurrentVisibleHeaders)
            {
                var cell = GetCellControl(rowId, columnHeader.Id);
                cell?.Refresh();
            }
        }
        
        public void ClearTable()
        {
            foreach (var rowHeader in _rowHeaderContainer.Children())
            {
                if (rowHeader is RowHeaderControl rowHeaderControl)
                {
                    VerticalResizer.Dispose(rowHeaderControl);
                    HeaderSwapper.Dispose(rowHeaderControl);
                    rowHeaderControl.RowControl.ClearRow();
                }
                
                rowHeader.style.height = 0;
            }
            _rowHeaders.Clear();
            _rowHeaderContainer.Clear();
            _rowData.Clear();
            _rowsContainer.Clear();

            foreach (var columnHeader in _columnHeaderContainer.Children())
            {
                if(columnHeader is ColumnHeaderControl columnHeaderControl)
                    HorizontalResizer.Dispose(columnHeaderControl);
                
                columnHeader.style.width = 0;
            }
            _columnHeaderContainer.Clear();
            _columnData.Clear();
            _columnHeaders.Clear();
            
            Resizer.Clear();
            RowVisibilityManager.Clear();
            ColumnVisibilityManager.Clear();
            
            _cornerContainer.CornerControl.style.width = 0;
            _rowsContainer.style.left = 0;
            
            _scrollViewHeight = UiConstants.CellHeight;
            _scrollViewWidth = 0;
            
            Resizer.OnResize -= RefreshScrollViewSize;
            
            if(Parent != null)
            {
                Parent.TableControl.OnScrollviewSizeChanged -= InvokeOnScrollviewSizeChanged;
                return;
            };
            Root.UnregisterCallback<GeometryChangedEvent>(evt =>
            {
                Vector2 delta = new Vector2(evt.newRect.width - evt.oldRect.width, evt.newRect.height - evt.oldRect.height);
                OnScrollviewSizeChanged?.Invoke(delta);
            });
        }

        public void Invert()
        {
            if(Parent != null) return;
            
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
            Metadata.IsInverted = Inverted;
        }
        
        private ScrollView CreateScrollView()
        {
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.AddToClassList(USSClasses.Fill);
            scrollView.contentContainer.AddToClassList(USSClasses.TableScrollViewContent);
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
        
        private void InitializeGeometry()
        {
            Resizer.OnResize -= RefreshScrollViewSize;
            Resizer.OnResize += RefreshScrollViewSize;
            
            _scrollViewHeight = UiConstants.CellHeight;
            _scrollViewWidth = 0;
            
            Resizer.ResizeAll(true);
            
            if(Parent != null)
            {
                Parent.TableControl.OnScrollviewSizeChanged -= InvokeOnScrollviewSizeChanged;
                Parent.TableControl.OnScrollviewSizeChanged += InvokeOnScrollviewSizeChanged;
                return;
            }
            
            Root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                Vector2 delta = new Vector2(evt.newRect.width - evt.oldRect.width, evt.newRect.height - evt.oldRect.height);
                OnScrollviewSizeChanged?.Invoke(delta);
            });
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
                foreach (var row in rows)
                {
                    _columnData.Add(row.Id, new CellAnchorData(row));

                    var headerCell = new ColumnHeaderControl(row, this);
                    _columnHeaderContainer.Add(headerCell);
                    _columnHeaders.Add(row.Id, headerCell);
                }
            }
            
        }

        private void BuildRows()
        {
            SortedList<int, Row> rowPositions = new SortedList<int, Row>();
            int lastPosition = 0;
            
            if (!Inverted)
            {
                IReadOnlyList<Row> rows = TableData.OrderedRows;
                if(rows.Count == 0)
                    return;

                foreach (var row in rows)
                {
                    _rowData.Add(row.Id, new CellAnchorData(row));

                    var header = new RowHeaderControl(row, this);
                    _rowHeaders.Add(row.Id, header);
                    _rowHeaderContainer.Add(header);
                    HeaderSwapper.HandleSwapping(header);

                    var rowControl = new RowControl(row, this);
                    _rowsContainer.Add(rowControl);
                    header.RowControl = rowControl;

                    int storedPosition = Metadata.GetAnchorPosition(row.Id);
                    if (storedPosition != 0)
                    {
                        rowPositions.Add(storedPosition, row);
                    }
                }

                foreach (var row in rowPositions)
                {
                    MoveRow(row.Value.Position, row.Key, false);
                }
                
                RefreshPage();
            }
            else
            {
                foreach (var column in TableData.OrderedColumns)
                {
                    _rowData.Add(column.Id, new CellAnchorData(column));

                    var header = new RowHeaderControl(column, this);
                    _rowHeaders.Add(column.Id, header);
                    _rowHeaderContainer.Add(header);
                    HeaderSwapper.HandleSwapping(header);
                    
                    var rowControl = new RowControl(column, this);
                    _rowsContainer.Add(rowControl);
                    header.RowControl = rowControl;
                }
            }
            
        }
        
        private void RefreshScrollViewSize(Vector2 sizeDelta)
        {
            _scrollViewWidth += sizeDelta.x;
            _scrollViewHeight += sizeDelta.y;
            _rowsContainer.style.height = _scrollViewHeight - UiConstants.CellHeight;

            VisualElementResizer.ChangeSize(ScrollView.contentContainer, _scrollViewWidth, _scrollViewHeight,
                evt =>
                {
                    Vector2 delta = new Vector2(evt.newRect.size.x - evt.oldRect.size.x, evt.newRect.size.y - evt.oldRect.size.y);
                    
                    ScrollView.horizontalScroller.highValue = _scrollViewWidth - ScrollView.contentViewport.resolvedStyle.width;
                    ScrollView.horizontalScroller.value = Mathf.Min(_scrollViewWidth, ScrollView.horizontalScroller.value);
                    ScrollView.horizontalScroller.Adjust(ScrollView.contentViewport.resolvedStyle.width / ScrollView.horizontalScroller.highValue);

                    ScrollView.verticalScroller.highValue = _scrollViewHeight - ScrollView.contentViewport.resolvedStyle.height;
                    ScrollView.verticalScroller.value = Mathf.Min(_scrollViewHeight, ScrollView.verticalScroller.value);
                    ScrollView.verticalScroller.Adjust(ScrollView.contentViewport.resolvedStyle.height / ScrollView.verticalScroller.highValue);

                    OnScrollviewSizeChanged?.Invoke(delta);
                });
        }
        
        private void InvokeOnScrollviewSizeChanged(Vector2 delta)
        {
            OnScrollviewSizeChanged?.Invoke(delta);
        }

        public CellControl GetCellControl(int rowId, int columnId)
        {
            if (!_rowData.ContainsKey(rowId) || !_columnData.ContainsKey(columnId))
                return null;

            int rowIndex = _rowData[rowId].Position - 1;
            int columnIndex = _columnData[columnId].Position - 1;

            if (rowIndex < 0 || columnIndex < 0)
                return null;

            if (_rowsContainer[rowIndex] is RowControl rowControl && rowControl.childCount <= columnIndex)
            {
                rowControl.Refresh(_rowData[rowId].CellAnchor);
            }

            return _rowsContainer[rowIndex].ElementAt(columnIndex) as CellControl;
        }

        public void RebuildPage()
        {
            SetTable(TableData);
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

        public void MoveRow(int rowStartPos, int rowEndPos, bool refresh = true)
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
                    
                    CellAnchor nextRow = this.GetRowAtPosition(nextIndex + 1);
                    
                    Metadata.SetAnchorPosition(nextRow.Id, currentIndex + 1);
                    
                    _rowsContainer.SwapChildren(currentIndex, nextIndex);
                    _rowHeaderContainer.SwapChildren(currentIndex, nextIndex);

                    currentIndex = nextIndex;
                }
                
                CellAnchor startRow = this.GetRowAtPosition(rowStartPos);
                Metadata.SetAnchorPosition(startRow.Id, rowEndPos);
                
                TableData.MoveRow(rowStartPos, rowEndPos);
                if (refresh) RefreshPage(); 
            }
            else
            {
                bool isMovingUp = rowStartPos > rowEndPos;
                int currentPos = rowStartPos;
                
                while (currentPos != rowEndPos)
                {
                    var nextPos = isMovingUp ? currentPos - 1 : currentPos + 1;
                    
                    CellAnchor currentRow = this.GetRowAtPosition(currentPos);
                    CellAnchor nextRow = this.GetRowAtPosition(nextPos);
                    Metadata.SwapMetadata(currentRow, nextRow);

                    currentPos = nextPos;
                }
                
                TableData.MoveRow(rowStartPos, rowEndPos);
                if(refresh) RebuildPage();
            }
        }


        private void OnRowHeaderBecameVisible(HeaderControl header, int direction)
        {
            if (header is RowHeaderControl rowHeaderControl)
            {
                rowHeaderControl.RowControl.Refresh(rowHeaderControl.RowControl.Anchor);
            }
        }
        
        private void OnRowHeaderBecameInvisible(HeaderControl header, int direction)
        {
            if (header is RowHeaderControl rowHeaderControl)
            {
                rowHeaderControl.RowControl.ClearRow();
            }
        }
        
        private void OnColumnHeaderBecameVisible(HeaderControl header, int direction)
        {
            RowsContainerOffset = 0;
            int index = 0;
            
            if (direction == 1)
            {
                while (index < ColumnVisibilityManager.CurrentVisibleHeaders.Count - 1
                       && ColumnVisibilityManager.IsHeaderVisibilityLocked(ColumnVisibilityManager.CurrentVisibleHeaders[index])
                       && !ColumnVisibilityManager.IsHeaderInBounds(ColumnVisibilityManager.CurrentVisibleHeaders[index]))
                {
                    index++;
                }
            }
            else
            {
                index = ColumnVisibilityManager.CurrentVisibleHeaders.Count - 1;

                while (index > 0
                       && ColumnVisibilityManager.IsHeaderVisibilityLocked(ColumnVisibilityManager.CurrentVisibleHeaders[index]) 
                       && !ColumnVisibilityManager.IsHeaderInBounds(ColumnVisibilityManager.CurrentVisibleHeaders[index]))
                {
                    index--;
                }
            }

            for (int i = 1; i < ColumnVisibilityManager.CurrentVisibleHeaders[index].CellAnchor.Position; i++)
            {
                ColumnHeaderControl columnHeader = _columnHeaders[this.GetColumnAtPosition(i).Id];
                if(ColumnVisibilityManager.IsHeaderVisibilityLocked(columnHeader)) continue;
                RowsContainerOffset += columnHeader.style.width.value.value;
            }

            _rowsContainer.style.left = RowsContainerOffset + _cornerContainer.CornerControl.style.width.value.value;
            

            if(ColumnVisibilityManager.IsHeaderVisibilityLocked(header as ColumnHeaderControl)) return;
            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
                rowHeader.RowControl.SetColumnVisibility(header.Id, true, direction);
            }
        }
        
        private void OnColumnHeaderBecameInvisible(HeaderControl header, int direction)
        {
            if (direction == 1 && ColumnVisibilityManager.CurrentVisibleHeaders.Any())
            {
                RowsContainerOffset = 0;
                int index = 0;
                
                while (index < ColumnVisibilityManager.CurrentVisibleHeaders.Count - 1
                       && ColumnVisibilityManager.IsHeaderVisibilityLocked(
                           ColumnVisibilityManager.CurrentVisibleHeaders[index])
                       && !ColumnVisibilityManager.IsHeaderInBounds(
                           ColumnVisibilityManager.CurrentVisibleHeaders[index]))
                {
                    index++;
                }


                for (int i = 1; i < ColumnVisibilityManager.CurrentVisibleHeaders[index].CellAnchor.Position; i++)
                {
                    ColumnHeaderControl columnHeader = _columnHeaders[this.GetColumnAtPosition(i).Id];
                    if (ColumnVisibilityManager.IsHeaderVisibilityLocked(columnHeader)) continue;
                    RowsContainerOffset += columnHeader.style.width.value.value;
                }

                _rowsContainer.style.left =
                    RowsContainerOffset + _cornerContainer.CornerControl.style.width.value.value;
            }

            if(ColumnVisibilityManager.IsHeaderVisibilityLocked(header as ColumnHeaderControl)) return;
            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
                rowHeader.RowControl.SetColumnVisibility(header.Id, false, direction);
            }
        }
    }
}
