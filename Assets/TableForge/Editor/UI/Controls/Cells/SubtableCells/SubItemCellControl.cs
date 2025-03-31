namespace TableForge.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.DynamicIfEmpty, TableReorderMode.ExplicitReorder, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellControl : ExpandableSubTableCellControl
    {
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
           
        }

        protected override void BuildSubTable()
        {
            SubTableControl = new TableControl(
                ParentTableControl.Root,
                CellStaticData.GetSubTableCellAttributes(GetType()), 
                this
            );
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            ContentContainer.Add(SubTableControl);

            if (Cell.GetValue() == null)
            {
                NullItemAddRowControl nullItemAddRow = new NullItemAddRowControl(SubTableControl);
                nullItemAddRow.OnRowAdded += () =>
                {
                    RecalculateSizeWithCurrentValues();
                    TableControl.VerticalResizer.ResizeCell(this);
                };
                
                ContentContainer.Add(nullItemAddRow);
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