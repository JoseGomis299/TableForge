namespace TableForge.UI
{
    internal static class CellExtensions
    {
        public static Cell GetRootCell(this Cell cell)
        {
            Cell parent = cell.Table.ParentCell;
            if(parent == null) return cell;
            
            return parent.GetRootCell();
        }
    }
}