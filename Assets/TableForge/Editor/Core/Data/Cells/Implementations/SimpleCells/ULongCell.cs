namespace TableForge.Editor
{
    /// <summary>
    /// Cell for unsigned long values.
    /// </summary>
    [CellType(typeof(ulong))]
    internal class ULongCell : PrimitiveBasedCell<ulong>, INumericBasedCell
    {
        public ULongCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}