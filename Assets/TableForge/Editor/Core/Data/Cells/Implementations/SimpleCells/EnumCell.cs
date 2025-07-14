using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Enum fields.
    /// </summary>
    internal class EnumCell : Cell, INumericBasedCell
    {
        public EnumCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new EnumCellSerializer(this);
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not EnumCell) return 1;
            return ((int)GetValue()).CompareTo((int)other.GetValue());
        }
    }
}