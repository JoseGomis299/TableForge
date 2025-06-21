namespace TableForge
{
    /// <summary>
    /// Cell for integer values.
    /// </summary>
    [CellType(typeof(int))]
    internal class IntCell : PrimitiveBasedCell<int>, INumericBasedCell
    {
        public IntCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}