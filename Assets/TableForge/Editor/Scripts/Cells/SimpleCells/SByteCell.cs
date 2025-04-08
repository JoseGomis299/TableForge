namespace TableForge
{
    [CellType(typeof(sbyte))]
    internal class SByteCell : Cell
    {
        public SByteCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}