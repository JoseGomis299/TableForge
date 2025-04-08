namespace TableForge
{
    /// <summary>
    /// Cell for double values
    /// </summary>
    [CellType(typeof(double))]
    internal class DoubleCell : Cell
    {
        public DoubleCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}