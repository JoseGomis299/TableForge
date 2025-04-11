namespace TableForge
{
    [CellType(typeof(ushort))]
    internal class UShortCell : Cell
    {
        public UShortCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}