namespace TableForge.UI
{
    internal class Vector4CellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public Vector4CellControl(Vector4Cell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _subTableControl = new TableControl(tableControl.Root);
            _subTableControl.SetTable(cell.SubTable);
            Add(_subTableControl);
            
            InitializeSize();
            IsSelected = false;
        }

        protected override void InitializeSize()
        {
            float width = 0, height = 0;
            foreach (var column in _subTableControl.ColumnData.Values)
            {
                width += column.PreferredWidth + 0.5f;
            }
            
            foreach (var row in _subTableControl.RowData.Values)
            {
                height += row.PreferredHeight + 0.5f;
            }
            
            SetDesiredSize(width, height);
        }
    }
}