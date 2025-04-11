namespace TableForge
{
    /// <summary>
    /// Cell for Enum fields.
    /// </summary>
    internal class EnumCell : Cell
    {
        public EnumCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}