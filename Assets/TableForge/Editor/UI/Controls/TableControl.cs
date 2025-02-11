using System.Collections.Generic;
using System.Linq;
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
        public VisualElement RowsContainer { get; }

        public HorizontalBorderResizer HorizontalResizer { get; }
        public VerticalBorderResizer VerticalResizer { get; }
        public CellSelector CellSelector { get; }

        public TableControl(VisualElement root)
        {
            Root = root;
            HorizontalResizer = new HorizontalBorderResizer(this);
            VerticalResizer = new VerticalBorderResizer(this);
            CellSelector = new CellSelector(this);

            ColumnHeaderContainer = new VisualElement();
            ColumnHeaderContainer.AddToClassList("table__row");

            RowsContainer = new VisualElement();
            RowsContainer.AddToClassList("table__row-container");

            Add(ColumnHeaderContainer);
            Add(RowsContainer);
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
            var headerCell = new ColumnHeaderControl(0, "", this);
            ColumnHeaderContainer.Add(headerCell);

            foreach (var columnEntry in TableData.Columns)
            {
                var column = columnEntry.Value;
                _columnData.Add(column.Id, new CellAnchorData(column));

                headerCell = new ColumnHeaderControl(column.Id, column.Name, this);
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
            }
        }
    }
}