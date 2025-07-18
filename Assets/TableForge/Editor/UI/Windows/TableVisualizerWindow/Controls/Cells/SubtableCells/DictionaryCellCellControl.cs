namespace TableForge.Editor.UI
{
    [CellControlUsage(typeof(DictionaryCell), CellSizeCalculationMethod.AutoSize)] 
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class DictionaryCellCellControl : DynamicSubTableCellControl
    {
        public DictionaryCellCellControl(DictionaryCell cell, TableControl tableControl) : base(cell, tableControl, new DefaultRowAdditionStrategy(), new RowDeletionStrategy())
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
            SubTableControl = new TableControl(
                parentTableControl.Root,
                CellStaticData.GetSubTableCellAttributes(GetType()), 
                this, subTableToolbar, parentTableControl.Visualizer
            );
            
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            SubTableControl.SetScrollbarsVisibility(false);
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