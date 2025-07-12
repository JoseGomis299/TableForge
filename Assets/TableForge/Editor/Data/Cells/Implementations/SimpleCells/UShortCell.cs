namespace TableForge.Editor
{
    [CellType(typeof(ushort))]
    internal class UShortCell : PrimitiveBasedCell<ushort>, INumericBasedCell
    {
        public UShortCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}