namespace TableForge.Editor
{
    /// <summary>
    /// Cell for long values.
    /// </summary>
    [CellType(typeof(long))]
    internal class LongCell : PrimitiveBasedCell<long>, INumericBasedCell
    {
        public LongCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}