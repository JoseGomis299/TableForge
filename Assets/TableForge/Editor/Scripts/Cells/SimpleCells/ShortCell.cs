namespace TableForge
{
    /// <summary>
    /// Represents a cell that contains a short integer value.
    /// </summary> 
    [CellType(typeof(short))]
    internal class ShortCell : PrimitiveBasedCell<short>, INumericBasedCell
    {
        public ShortCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}