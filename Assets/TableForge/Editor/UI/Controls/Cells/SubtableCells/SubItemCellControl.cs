namespace TableForge.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.DynamicIfEmpty, TableReorderMode.ExplicitReorder, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellControl : ExpandableSubTableCellControl
    {
        private NullItemAddRowControl _nullItemAddRow;
        
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }

        public override void ClearSubTable()
        {
            base.ClearSubTable();
            
            if(Cell.Table.Rows.Count != 0 && _nullItemAddRow != null)
            {
                _nullItemAddRow.RemoveFromHierarchy();
                _nullItemAddRow = null;
            }
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            
            if(IsSubTableInitialized && ((SubTableCell)Cell).SubTable.Rows.Count == 0 && _nullItemAddRow == null)
            {
                _nullItemAddRow = new NullItemAddRowControl(SubTableControl);
                _nullItemAddRow.OnRowAdded += () =>
                {
                    RecalculateSizeWithCurrentValues();
                    TableControl.VerticalResizer.ResizeCell(this);
                };
                
                SubTableContentContainer.Add(_nullItemAddRow);
            }
        }

        protected override void BuildSubTable()
        {
            SubTableControl = new TableControl(
                ParentTableControl.Root,
                CellStaticData.GetSubTableCellAttributes(GetType()), 
                this
            );
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            SubTableControl.SetScrollbarsVisibility(false);
            SubTableContentContainer.Add(SubTableControl);
            
            if(((SubTableCell)Cell).SubTable.Rows.Count == 0 && _nullItemAddRow == null)
            {
                _nullItemAddRow = new NullItemAddRowControl(SubTableControl);
                _nullItemAddRow.OnRowAdded += () =>
                {
                    RecalculateSizeWithCurrentValues();
                    TableControl.VerticalResizer.ResizeCell(this);
                };
                
                SubTableContentContainer.Add(_nullItemAddRow);
            }

            SubTableControl.HorizontalResizer.OnManualResize += _ =>
            {
                RecalculateSizeWithCurrentValues();
                TableControl.HorizontalResizer.ResizeCell(this);
            };
            SubTableControl.VerticalResizer.OnManualResize += _ =>
            {
                RecalculateSizeWithCurrentValues();
                TableControl.VerticalResizer.ResizeCell(this);
            };
        }
    }
}