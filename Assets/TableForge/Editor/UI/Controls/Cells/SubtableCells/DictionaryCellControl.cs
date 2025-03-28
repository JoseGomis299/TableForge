namespace TableForge.UI
{
    [CellControlUsage(typeof(DictionaryCell), CellSizeCalculationMethod.AutoSize)] 
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class DictionaryCellControl : ExpandableSubTableCellControl
    {
        public DictionaryCellControl(DictionaryCell cell, TableControl tableControl) : base(cell, tableControl)
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