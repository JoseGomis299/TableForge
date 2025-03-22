using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.ExplicitReorder, 
        TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellControl : ExpandableSubTableCellControl
    {
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
           
        }

        protected override void InitializeSubTable()
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
                RecalculateSize();
                TableControl.HorizontalResizer.ResizeCell(this);
            };
            SubTableControl.VerticalResizer.OnResize += _ =>
            {
                RecalculateSize();
                TableControl.VerticalResizer.ResizeCell(this);
            };
        }
    }
}