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
                ContentContainer.Add(new NullItemAddRowControl(SubTableControl));

            SubTableControl.HorizontalResizer.OnResize += _ =>
            {
                RecalculateSizeWithCurrentValues();
                TableControl.HorizontalResizer.ResizeCell(this);
            };
            SubTableControl.VerticalResizer.OnResize += _ =>
            {
                RecalculateSizeWithCurrentValues();
                TableControl.VerticalResizer.ResizeCell(this);
            };
        }
    }
}