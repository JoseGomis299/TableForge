namespace TableForge.Editor
{
    [CellType(typeof(byte))]
    internal class ByteCell : PrimitiveBasedCell<byte>, INumericBasedCell
    {
        public ByteCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}