using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableControl : VisualElement
    {
        private readonly Dictionary<int, CellAnchorData> _columnData = new Dictionary<int, CellAnchorData>();
        private readonly Dictionary<int, CellAnchorData> _rowData = new Dictionary<int, CellAnchorData>();
        private readonly Dictionary<int, RowHeaderControl> _rowHeaders = new Dictionary<int, RowHeaderControl>();
        private readonly Dictionary<int, ColumnHeaderControl> _columnHeaders = new Dictionary<int, ColumnHeaderControl>();
        
        public IReadOnlyDictionary<int, CellAnchorData> ColumnData => _columnData;
        public IReadOnlyDictionary<int, CellAnchorData> RowData => _rowData;
        public IReadOnlyDictionary<int, RowHeaderControl> RowHeaders => _rowHeaders;
        public IReadOnlyDictionary<int, ColumnHeaderControl> ColumnHeaders => _columnHeaders;
        
        public Table TableData { get; private set; }
        public VisualElement Root { get; }

        public VisualElement ColumnHeaderContainer { get; }
        public VisualElement RowHeaderContainer { get; }
        public VisualElement CornerContainer { get; }
        public VisualElement RowsContainer { get; }
        public ScrollView ScrollView { get; }

        public HorizontalBorderResizer HorizontalResizer { get; }
        public VerticalBorderResizer VerticalResizer { get; }
        public CellSelector CellSelector { get; }
        
        public CellVisibilityManager CellVisibilityManager { get; }

        public TableControl(VisualElement root)
        {
            Root = root;
            AddToClassList("table");

            ScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            ScrollView.AddToClassList("fill");
            CellVisibilityManager = new CellVisibilityManager(this);
            
            HorizontalResizer = new HorizontalBorderResizer(this, CellVisibilityManager);
            VerticalResizer = new VerticalBorderResizer(this);
            
            RowsContainer = new VisualElement();
            RowsContainer.AddToClassList("table__row-container");
            
            ColumnHeaderContainer = new ColumnHeaderContainerControl(ScrollView);
            RowHeaderContainer = new RowHeaderContainerControl(ScrollView);
            CornerContainer = new CornerContainerControl(ScrollView);
            
            CellSelector = new CellSelector(this);
          
            Add(ScrollView);
            VisualElement parent = new VisualElement();
            parent.AddToClassList("fill");
            parent.style.flexDirection = FlexDirection.ColumnReverse;
            ScrollView.Add(parent);
            
            VisualElement rowAndHeaderContainer = new VisualElement();
            rowAndHeaderContainer.style.flexShrink = 1;
            rowAndHeaderContainer.style.flexDirection = FlexDirection.Row;
            
            parent.Add(rowAndHeaderContainer);
            rowAndHeaderContainer.Add(RowsContainer);
            rowAndHeaderContainer.Add(RowHeaderContainer);
            
            VisualElement cornerAndHeaderContainer = new VisualElement();
            cornerAndHeaderContainer.style.flexDirection = FlexDirection.Row;
            cornerAndHeaderContainer.style.flexShrink = 0;
            
            parent.Add(cornerAndHeaderContainer);
            var cornerCell = new TableCornerControl(this);
            CornerContainer.Add(cornerCell);
            cornerAndHeaderContainer.Add(ColumnHeaderContainer);
            cornerAndHeaderContainer.Add(CornerContainer);
        }

        public void SetTable(Table table)
        {
            TableData = table;

            _columnData.Clear();
            _rowData.Clear();
            _rowHeaders.Clear();
            _columnHeaders.Clear();
            ColumnHeaderContainer.Clear();
            RowsContainer.Clear();
            BuildHeader();
            BuildRows();
            
            HorizontalResizer.ResizeAll();
            VerticalResizer.ResizeAll();
        }
        
        
        private void BuildHeader()
        {
            _columnData.Add(0, new CellAnchorData(null));

            foreach (var columnEntry in TableData.Columns)
            {
                var column = columnEntry.Value;
                _columnData.Add(column.Id, new CellAnchorData(column));

                var headerCell = new ColumnHeaderControl(column.Id, column.Name, this);
                ColumnHeaderContainer.Add(headerCell);
                _columnHeaders.Add(column.Id, headerCell);
            }
        }

        private void BuildRows()
        {
            foreach (var rowEntry in TableData.Rows)
            {
                var row = rowEntry.Value;
                _rowData.Add(row.Id, new CellAnchorData(row));

                var rowControl = new TableRowControl(row, this);
                RowsContainer.Add(rowControl);
                
                _rowHeaders.Add(row.Id, rowControl.Children().First() as RowHeaderControl);
                RowHeaderContainer.Add(rowControl.Children().First());
            }
        }
    }
}