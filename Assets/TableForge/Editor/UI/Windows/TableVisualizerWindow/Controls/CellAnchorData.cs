namespace TableForge.UI
{
    internal class CellAnchorData
    {
        public CellAnchor CellAnchor { get; }

        public int Position => CellAnchor?.Position ?? 0;
        public string Id => CellAnchor?.Id ?? string.Empty;
        
        public CellAnchorData(CellAnchor cellAnchor)
        {
            CellAnchor = cellAnchor;
        }
    }
}