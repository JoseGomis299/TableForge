using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Fallback cell type for unsupported data types.
    /// </summary>
    internal class DefaultCell : Cell
    {
        public DefaultCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new DefaultCellSerializer(this);
        }
        
        public override int CompareTo(Cell other)
        {
            return -1;
        }
    }
}