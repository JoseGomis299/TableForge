namespace TableForge
{
    /// <summary>
    /// Fallback cell type for unsupported data types.
    /// </summary>
    internal class DefaultCell : Cell
    {
        public DefaultCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}