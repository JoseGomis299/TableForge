namespace TableForge
{
    /// <summary>
    /// Cell for long values.
    /// </summary>
    [CellType(typeof(long))]
    internal class LongCell : PrimitiveBasedCell<long>
    {
        public LongCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}