namespace TableForge.UI
{
    internal abstract class SubTableCellControl : CellControl
    {
        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }
    }
}