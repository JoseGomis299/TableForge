namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.ImplicitReorder, TableHeaderVisibility.ShowHeaderNumberBase0, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellCellControl : DynamicSubTableCellControl
    {
        public ListCellCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl, new ListRowAdditionStrategy(), new RowDeletionStrategy())
        {
          
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            if(SubTableControl?.TableData == null) return;
            ShowDeleteRowButton(SubTableControl?.TableData.Rows.Count > 0);
        }

        protected override void BuildSubTable()
        {
            SubTableControl = new TableControl(parentTableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this, subTableToolbar, parentTableControl.Visualizer);
            SubTableControl.SetScrollbarsVisibility(false);
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            subTableContentContainer.Add(SubTableControl);
            
            ShowAddRowButton(true);
            ShowDeleteRowButton(SubTableControl.TableData.Rows.Count > 0);

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