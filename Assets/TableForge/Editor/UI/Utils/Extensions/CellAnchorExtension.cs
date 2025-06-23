namespace TableForge.Editor.UI
{
    internal static class CellAnchorExtension
    {
        public static CellAnchor GetRootAnchor(this CellAnchor anchor)
        {
            if (anchor == null)
                return null;

            bool isRow = anchor is Row;
            CellAnchor current = anchor;

            while (current.Table.ParentCell is SubTableCell parentCell)
            {
                current = isRow ? parentCell.row : parentCell.column;
            }

            return current;
        }
    }
}