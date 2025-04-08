namespace TableForge
{
    /// <summary>
    /// Cell for integer values.
    /// </summary>
    [CellType(typeof(int))]
    internal class IntCell : Cell
    {
        public IntCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}