using TableForge.Editor;

namespace TableForge.Editor
{
    [CellType(typeof(byte))]
    internal class ByteCell : PrimitiveBasedCell<byte>, INumericBasedCell
    {
        public ByteCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}