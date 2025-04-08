namespace TableForge
{
    [CellType(typeof(short))]
    internal class ShortCell : Cell
    {
        public ShortCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}