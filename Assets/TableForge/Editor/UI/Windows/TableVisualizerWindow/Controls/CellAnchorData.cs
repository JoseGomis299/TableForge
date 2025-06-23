namespace TableForge.Editor.UI
{
    internal class CellAnchorData
    {
        public CellAnchor CellAnchor { get; }

        public int Position => CellAnchor?.Position ?? 0;
        public int Id => CellAnchor?.Id ?? 0;
        
        public CellAnchorData(CellAnchor cellAnchor)
        {
            CellAnchor = cellAnchor;
        }
    }
}