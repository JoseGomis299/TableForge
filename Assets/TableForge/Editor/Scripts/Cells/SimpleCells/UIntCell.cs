namespace TableForge
{
    /// <summary>
    /// Cell for uint values
    /// </summary>
    [CellType(typeof(uint))]
    internal class UIntCell : Cell
    {
        public UIntCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}