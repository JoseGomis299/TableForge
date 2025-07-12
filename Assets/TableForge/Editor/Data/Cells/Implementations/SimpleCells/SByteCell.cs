namespace TableForge.Editor
{
    /// <summary>
    /// Cell that contains a signed byte value.
    /// </summary>
    [CellType(typeof(sbyte))]
    internal class SByteCell : PrimitiveBasedCell<sbyte>, INumericBasedCell
    {
        public SByteCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}