namespace TableForge
{
    /// <summary>
    /// Cell for unsigned long values.
    /// </summary>
    [CellType(typeof(ulong))]
    internal class ULongCell : Cell
    {
        public ULongCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}