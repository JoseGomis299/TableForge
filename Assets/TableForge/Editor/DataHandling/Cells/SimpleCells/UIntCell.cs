using TableForge.Editor;

namespace TableForge
{
    /// <summary>
    /// Cell for uint values
    /// </summary>
    [CellType(typeof(uint))]
    internal class UIntCell : PrimitiveBasedCell<uint>, INumericBasedCell
    {
        public UIntCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}