namespace TableForge
{
    [CellType(typeof(ushort))]
    internal class UShortCell : PrimitiveBasedCell<ushort>, INumericBasedCell
    {
        public UShortCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}