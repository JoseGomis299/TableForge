namespace TableForge
{
    [CellType(typeof(byte))]
    internal class ByteCell : PrimitiveBasedCell<byte>
    {
        public ByteCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}