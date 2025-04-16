using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableControl : VisualElement
    {
        #region Fields

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
        
        private List<ColumnHeaderControl> _orderedColumnHeaders = new();
        private List<ColumnHeaderControl> _orderedDescColumnHeaders = new();
        private List<RowHeaderControl> _orderedRowHeaders = new();
        private List<RowHeaderControl> _orderedDescRowHeaders = new();
        

        #endregion

        #region Properties

        public IReadOnlyDictionary<int, CellAnchorData> ColumnData => _columnData;
        public IReadOnlyDictionary<int, CellAnchorData> RowData => _rowData;
        public IReadOnlyDictionary<int, RowHeaderControl> RowHeaders => _rowHeaders;
        public IReadOnlyDictionary<int, ColumnHeaderControl> ColumnHeaders => _columnHeaders;
        public IReadOnlyList<ColumnHeaderControl> OrderedColumnHeaders => _orderedColumnHeaders;
        public IReadOnlyList<RowHeaderControl> OrderedRowHeaders => _orderedRowHeaders;
        public IReadOnlyList<RowHeaderControl> OrderedDescRowHeaders => _orderedDescRowHeaders;
        public IReadOnlyList<ColumnHeaderControl> OrderedDescColumnHeaders => _orderedDescColumnHeaders;

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
        public bool Transposed { get; private set; }
        public float RowsContainerOffset { get; private set; }

        #endregion

        #region Constructor

        public TableControl(VisualElement root, TableAttributes attributes, SubTableCellControl parent)
        {
            // Basic initialization
            Root = root;
            TableAttributes = attributes;
            Parent = parent;
            AddToClassList(USSClasses.Table);

            // Initialize main components
            ScrollView = CreateScrollView();
            Resizer = new TableResizer(this);
            ColumnVisibilityManager = new ColumnVisibilityManager(this);
            RowVisibilityManager = new RowVisibilityManager(this);
            CellSelector = parent != null ? parent.TableControl.CellSelector : new CellSelector(this);
            HeaderSwapper = new HeaderSwapper(this);

            // Initialize sub-containers
            _rowsContainer = CreateRowsContainer();
            _columnHeaderContainer = new ColumnHeaderContainerControl(ScrollView);
            _rowHeaderContainer = new RowHeaderContainerControl(ScrollView);
            _cornerContainer = new CornerContainerControl(ScrollView);

            // Subscribe to visibility events
            SubscribeToVisibilityEvents();

            // Build UI hierarchy (styles defined in USS)
            BuildLayoutHierarchy();
        }

        #endregion

        #region Public Methods

        #region Table Setup

        public void SetTable(Table table)
        {
            TableData = table;
            Metadata = Parent == null
                ? TableMetadataManager.GetMetadata(table, table.Name)
                : Parent.TableControl.Metadata;
            
            if (!Transposed && Metadata.IsTransposed)
                Transpose();

            if (_columnData.Any())
                ClearTable();

            PreferredSize = SizeCalculator.CalculateTableSize(table, TableAttributes, Metadata);

            // Add empty data for the corner cell
            _rowData.Add(0, new CellAnchorData(null));
            _columnData.Add(0, new CellAnchorData(null));

            BuildColumns();
            BuildRows();
            InitializeGeometry();
        }

        public void ClearTable()
        {
            ClearRows();
            ClearColumns();
            Resizer.Clear();
            RowVisibilityManager.Clear();
            ColumnVisibilityManager.Clear();

            _cornerContainer.CornerControl.style.width = 0;
            _rowsContainer.style.left = 0;

            ResetScrollViewStoredSize();
            UnsubscribeFromResizingMethods();
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates the data of the visible rows.
        /// </summary>
        public void Update()
        {
            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
               rowHeader.Refresh();
            }
        }
        
        /// <summary>
        /// Updates the data of all rows but only updating visible cells values.
        /// </summary>
        public void UpdateAll()
        {
            foreach (var row in _rowHeaders.Values)
            {
                row.Refresh();
            }
        }

        /// <summary>
        /// Updates the data of this row and its visible cells.
        /// </summary>
        public void UpdateRow(int rowId)
        {
            if (!_rowHeaders.TryGetValue(rowId, out var header))
                return;

            header.Refresh();
        }
        
        public void RebuildPage() => SetTable(TableData);
        #endregion

        #region Utilities

        public void Transpose()
        {
            if (Parent != null)
                return;

            Transposed = !Transposed;
            TableAttributes = new TableAttributes
            {
                ColumnHeaderVisibility = TableAttributes.RowHeaderVisibility,
                RowHeaderVisibility = TableAttributes.ColumnHeaderVisibility,
                RowReorderMode = TableAttributes.ColumnReorderMode,
                ColumnReorderMode = TableAttributes.RowReorderMode,
                TableType = TableAttributes.TableType
            };
            
            Metadata.IsTransposed = Transposed;
        }


        public void MoveRow(int rowStartPos, int rowEndPos, bool refresh = true)
        {
            if (rowStartPos == rowEndPos)
                return;

            if (TableAttributes.RowReorderMode == TableReorderMode.ExplicitReorder)
            {
                PerformExplicitRowReorder(rowStartPos, rowEndPos, refresh);
            }
            else
            {
                PerformImplicitRowReorder(rowStartPos, rowEndPos, refresh);
            }
            
            _orderedRowHeaders = RowHeaders.Values.OrderBy(x => x.CellAnchor.Position).ToList();
            _orderedDescRowHeaders = RowHeaders.Values.OrderByDescending(x => x.CellAnchor.Position).ToList();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Layout Initialization

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
            var scrollviewContentContainer = new VisualElement();
            scrollviewContentContainer.AddToClassList(USSClasses.TableScrollViewContentContainer);
            ScrollView.Add(scrollviewContentContainer);

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
            SubscribeToResizingMethods();
            ResetScrollViewStoredSize();
            Resizer.ResizeAll(true);
        }
        
        private void ResetScrollViewStoredSize()
        {
            _scrollViewHeight = UiConstants.CellHeight;
            _scrollViewWidth = 0;
        }

        #endregion
        
        #region Table structure building

        private void BuildColumns()
        {
            if (!Transposed)
            {
                foreach (var column in TableData.OrderedColumns)
                {
                    BuildColumn(column);
                }
            }
            else
            {
                foreach (var column in TableData.OrderedRows)
                {
                    BuildColumn(column);
                }
            }
            
            _orderedColumnHeaders = ColumnHeaders.Values.OrderBy(x => x.CellAnchor.Position).ToList();
            _orderedDescColumnHeaders = ColumnHeaders.Values.OrderByDescending(x => x.CellAnchor.Position).ToList();
        }

        private void BuildColumn<T>(T column) where T : CellAnchor
        {
            _columnData.Add(column.Id, new CellAnchorData(column));
                
            var headerCell = new ColumnHeaderControl(column, this);
            _columnHeaderContainer.Add(headerCell);
            _columnHeaders.Add(column.Id, headerCell);
        }

        private void BuildRows()
        {
            SortedList<int, Row> rowPositions = new SortedList<int, Row>();

            if (!Transposed)
            {
                var rows = TableData.OrderedRows;
                if (rows.Count == 0)
                    return;

                foreach (var row in rows)
                {
                    BuildRow(row);

                    //If the row position is not 0, it means it has been moved during other sessions
                    int storedPosition = Metadata.GetAnchorPosition(row.Id);
                    if (storedPosition != 0)
                        rowPositions.Add(storedPosition, row);
                }

                // Move rows to their stored positions
                foreach (var row in rowPositions)
                {
                    MoveRow(row.Value.Position, row.Key, false);
                }

                UpdateAll();
            }
            else
            {
                foreach (var column in TableData.OrderedColumns)
                {
                    BuildRow(column);
                }
            }
            
            _orderedRowHeaders = RowHeaders.Values.OrderBy(x => x.CellAnchor.Position).ToList();
            _orderedDescRowHeaders = RowHeaders.Values.OrderByDescending(x => x.CellAnchor.Position).ToList();
        }
        
        private void BuildRow<T>(T row) where T : CellAnchor
        {
            _rowData.Add(row.Id, new CellAnchorData(row));
            var header = new RowHeaderControl(row, this);
            _rowHeaders.Add(row.Id, header);
            _rowHeaderContainer.Add(header);
            HeaderSwapper.HandleSwapping(header);

            var rowControl = new RowControl(row, this);
            _rowsContainer.Add(rowControl);
            header.RowControl = rowControl;
        }

        #endregion

        #region Helper methods

        #region Row reordering helpers
        private void PerformExplicitRowReorder(int rowStartPos, int rowEndPos, bool refresh)
        {
            int startIndex = rowStartPos - 1;
            int endIndex = rowEndPos - 1;
            bool isMovingUp = rowStartPos > rowEndPos;
            int currentIndex = startIndex;

            while (currentIndex != endIndex)
            {
                int nextIndex = isMovingUp ? currentIndex - 1 : currentIndex + 1;
                var nextRow = this.GetRowAtPosition(nextIndex + 1);
                Metadata.SetAnchorPosition(nextRow.Id, currentIndex + 1);

                _rowsContainer.SwapChildren(currentIndex, nextIndex);
                _rowHeaderContainer.SwapChildren(currentIndex, nextIndex);

                currentIndex = nextIndex;
            }

            var startRow = this.GetRowAtPosition(rowStartPos);
            Metadata.SetAnchorPosition(startRow.Id, rowEndPos);
            TableData.MoveRow(rowStartPos, rowEndPos);
            if (refresh) UpdateAll();
        }

        private void PerformImplicitRowReorder(int rowStartPos, int rowEndPos, bool refresh)
        {
            bool isMovingUp = rowStartPos > rowEndPos;
            int currentPos = rowStartPos;

            while (currentPos != rowEndPos)
            {
                int nextPos = isMovingUp ? currentPos - 1 : currentPos + 1;
                var currentRow = this.GetRowAtPosition(currentPos);
                var nextRow = this.GetRowAtPosition(nextPos);
                Metadata.SwapMetadata(currentRow, nextRow);
                currentPos = nextPos;
            }

            TableData.MoveRow(rowStartPos, rowEndPos);
            if (refresh)
            {
                CellSelector.ClearSelection();
                RebuildPage();
            }
        }
        #endregion

        #region Clearing helpers
        private void ClearRows()
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
        }

        private void ClearColumns()
        {
            foreach (var columnHeader in _columnHeaderContainer.Children())
            {
                if (columnHeader is ColumnHeaderControl columnHeaderControl)
                    HorizontalResizer.Dispose(columnHeaderControl);
                columnHeader.style.width = 0;
            }
            _columnHeaderContainer.Clear();
            _columnData.Clear();
            _columnHeaders.Clear();
        }
        #endregion
        
        #region Subscription helpers
        private void SubscribeToVisibilityEvents()
        {
            RowVisibilityManager.OnHeaderBecameVisible += OnRowHeaderBecameVisible;
            RowVisibilityManager.OnHeaderBecameInvisible += OnRowHeaderBecameInvisible;
            ColumnVisibilityManager.OnHeaderBecameVisible += OnColumnHeaderBecameVisible;
            ColumnVisibilityManager.OnHeaderBecameInvisible += OnColumnHeaderBecameInvisible;
        }

        private void InvokeOnScrollviewSizeChanged(Vector2 delta)
        {
            OnScrollviewSizeChanged?.Invoke(delta);
        }
        
        private void SubscribeToResizingMethods()
        {
            Resizer.OnResize -= OnTableResize;
            Resizer.OnResize += OnTableResize;

            if (Parent != null)
            {
                Parent.TableControl.OnScrollviewSizeChanged -= InvokeOnScrollviewSizeChanged;
                Parent.TableControl.OnScrollviewSizeChanged += InvokeOnScrollviewSizeChanged;
                return;
            }
            Root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
        }

        private void UnsubscribeFromResizingMethods()
        {
            Resizer.OnResize -= OnTableResize;

            if (Parent != null)
            {
                Parent.TableControl.OnScrollviewSizeChanged -= InvokeOnScrollviewSizeChanged;
                return;
            }
            Root.UnregisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
        }
        #endregion

        // Helper for calculating offset based on visible columns
        private float CalculateRowsContainerOffset(int direction)
        {
            float offset = 0;
            int index = direction == 1 ? 0 : ColumnVisibilityManager.CurrentVisibleHeaders.Count - 1;

            if (direction == 1)
            {
                while (index < ColumnVisibilityManager.CurrentVisibleHeaders.Count - 1 &&
                       ColumnVisibilityManager.IsHeaderVisibilityLocked(ColumnVisibilityManager.CurrentVisibleHeaders[index]) &&
                       !ColumnVisibilityManager.IsHeaderInBounds(ColumnVisibilityManager.CurrentVisibleHeaders[index], true))
                {
                    index++;
                }
            }
            else
            {
                while (index > 0 &&
                       ColumnVisibilityManager.IsHeaderVisibilityLocked(ColumnVisibilityManager.CurrentVisibleHeaders[index]) &&
                       !ColumnVisibilityManager.IsHeaderInBounds(ColumnVisibilityManager.CurrentVisibleHeaders[index], true))
                {
                    index--;
                }
            }

            for (int i = 1; i < ColumnVisibilityManager.CurrentVisibleHeaders[index].CellAnchor.Position; i++)
            {
                CellAnchor column = this.GetColumnAtPosition(i);
                if (column == null || !_columnHeaders.TryGetValue(column.Id, out var columnHeader))
                    continue;
                
                if (ColumnVisibilityManager.IsHeaderVisibilityLocked(columnHeader))
                    continue;
                offset += columnHeader.style.width.value.value;
            }
            return offset;
        }

        #endregion

        #region Event Handlers

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
            RowsContainerOffset = CalculateRowsContainerOffset(direction);
            _rowsContainer.style.left = RowsContainerOffset + _cornerContainer.CornerControl.style.width.value.value;

            if (ColumnVisibilityManager.IsHeaderVisibilityLocked(header as ColumnHeaderControl))
                return;

            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
                rowHeader.RowControl.SetColumnVisibility(header.Id, true, direction);
            }
        }

        private void OnColumnHeaderBecameInvisible(HeaderControl header, int direction)
        {
            if (direction == 1 && ColumnVisibilityManager.CurrentVisibleHeaders.Any())
            {
                RowsContainerOffset = CalculateRowsContainerOffset(direction);
                _rowsContainer.style.left = RowsContainerOffset + _cornerContainer.CornerControl.style.width.value.value;
            }

            if (ColumnVisibilityManager.IsHeaderVisibilityLocked(header as ColumnHeaderControl))
                return;

            foreach (var rowHeader in RowVisibilityManager.CurrentVisibleHeaders)
            {
                rowHeader.RowControl.SetColumnVisibility(header.Id, false, direction);
            }
        }
        
        private void OnTableResize(Vector2 sizeDelta)
        {
            _scrollViewWidth += sizeDelta.x;
            _scrollViewHeight += sizeDelta.y;
            _rowsContainer.style.height = _scrollViewHeight - UiConstants.CellHeight;

            VisualElementResizer.ChangeSize(ScrollView.contentContainer, _scrollViewWidth, _scrollViewHeight, OnContentContainerResized);
        }

        private void OnContentContainerResized(GeometryChangedEvent evt)
        {
            Vector2 delta = new Vector2(evt.newRect.size.x - evt.oldRect.size.x, evt.newRect.size.y - evt.oldRect.size.y);

            //Adjust scrollers
            ScrollView.SetHorizontalScrollerValue(_scrollViewWidth);
            ScrollView.SetVerticalScrollerValue(_scrollViewHeight);

            OnScrollviewSizeChanged?.Invoke(delta);
        }
        
        private void OnRootGeometryChanged(GeometryChangedEvent evt)
        {
            Vector2 delta = new Vector2(evt.newRect.width - evt.oldRect.width, evt.newRect.height - evt.oldRect.height);
            OnScrollviewSizeChanged?.Invoke(delta);
        }

        #endregion

        #endregion
    }
}
