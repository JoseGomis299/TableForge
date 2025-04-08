namespace TableForge
{
    [CellType(typeof(ushort))]
    internal class UShortCell : Cell
    {
        public UShortCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}