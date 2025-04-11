namespace TableForge
{
    /// <summary>
    /// Cell that contains a signed byte value.
    /// </summary>
    [CellType(typeof(sbyte))]
    internal class SByteCell : Cell
    {
        public SByteCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}