namespace TableForge
{
    /// <summary>
    /// Cell for uint values
    /// </summary>
    [CellType(typeof(uint))]
    internal class UIntCell : PrimitiveBasedCell<uint>
    {
        public UIntCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}