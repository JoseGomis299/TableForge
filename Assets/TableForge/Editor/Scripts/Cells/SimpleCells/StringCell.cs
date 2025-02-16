namespace TableForge
{
    /// <summary>
    /// Cell for string type fields.
    /// </summary>
    [CellType(typeof(string))]
    internal class StringCell : Cell
    {
        public StringCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}