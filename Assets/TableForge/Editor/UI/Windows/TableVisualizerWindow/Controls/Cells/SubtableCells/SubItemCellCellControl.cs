namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.DynamicIfEmpty, TableReorderMode.ExplicitReorder, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellCellControl : DynamicSubTableCellControl
    {
        public SubItemCellCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl, new NullItemRowAdditionStrategy(), new RowDeletionStrategy())
        {
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            ShowAddRowButton(IsSubTableInitialized && ((SubTableCell)Cell).SubTable.Rows.Count == 0);
        }

        protected override void BuildSubTable()
        {
            SubTableControl = new TableControl(
                parentTableControl.Root,
                CellStaticData.GetSubTableCellAttributes(GetType()), 
                this, subTableToolbar, parentTableControl.Visualizer
            );
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            SubTableControl.SetScrollbarsVisibility(false);
            subTableContentContainer.Add(SubTableControl);
            
            ShowAddRowButton(((SubTableCell)Cell).SubTable.Rows.Count == 0);

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

        public override void OnRowAdded()
        {
            RecalculateSizeWithCurrentValues();
            TableControl.VerticalResizer.ResizeCell(this);
            if(SubTableControl != null)
            {
                ShowAddRowButton(false);
                subTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
            }
        }
    }
}