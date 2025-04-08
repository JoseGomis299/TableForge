namespace TableForge
{
    [CellType(typeof(byte))]
    internal class ByteCell : Cell
    {
        public ByteCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}