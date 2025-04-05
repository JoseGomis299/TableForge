namespace TableForge
{
    /// <summary>
    /// Cell for long values.
    /// </summary>
    [CellType(typeof(long))]
    internal class LongCell : Cell
    {
        public LongCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}