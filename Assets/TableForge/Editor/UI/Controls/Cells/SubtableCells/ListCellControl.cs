namespace TableForge.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.ImplicitReorder, TableHeaderVisibility.ShowHeaderNumberBase0, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellControl : ExpandableSubTableCellControl
    {
        public ListCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl)
        {
          
        }

        protected override void BuildSubTable()
        {
            SubTableControl = new TableControl(ParentTableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this);
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);

            ListAddRowControl listAddRowControl = new ListAddRowControl(SubTableControl);
            listAddRowControl.OnRowAdded += () =>
            {
                RecalculateSizeWithCurrentValues();
                TableControl.Resizer.ResizeCell(this);
            };
            
            ContentContainer.Add(SubTableControl);
            ContentContainer.Add(listAddRowControl);
            
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