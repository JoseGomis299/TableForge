using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.ImplicitReorder, TableHeaderVisibility.ShowHeaderNumberBase0, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellControl : ExpandableSubTableCellControl
    {
        public ListCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl)
        {
          
        }

        protected override void InitializeSubTable()
        {
            SubTableControl = new TableControl(ParentTableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this);
            SubTableControl.SetTable(((SubTableCell)Cell).SubTable);

            ContentContainer.Add(SubTableControl);
            ContentContainer.Add(new ListAddRowControl(SubTableControl));
            
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