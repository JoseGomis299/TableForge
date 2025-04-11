namespace TableForge
{
    /// <summary>
    /// Cell for boolean type fields.
    /// </summary>
    [CellType(typeof(bool))]
    internal class BoolCell : Cell
    {
        public BoolCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}