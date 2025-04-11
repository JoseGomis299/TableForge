namespace TableForge
{
    [CellType(typeof(byte))]
    internal class ByteCell : Cell
    {
        public ByteCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}