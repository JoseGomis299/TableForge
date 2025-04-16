namespace TableForge
{
    /// <summary>
    /// Cell for float values 
    /// </summary>
    [CellType(typeof(float))]
    internal class FloatCell : PrimitiveBasedCell<float>
    {
        public FloatCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}