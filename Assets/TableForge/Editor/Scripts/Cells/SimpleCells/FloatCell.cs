namespace TableForge
{
    /// <summary>
    /// Cell for float values 
    /// </summary>
    [CellType(typeof(float))]
    internal class FloatCell : Cell
    {
        public FloatCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}