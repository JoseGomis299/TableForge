using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    [CellType(typeof(char))]
    internal class CharCell : PrimitiveBasedCell<char>, INumericBasedCell
    {
        public CharCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new CharCellSerializer(this);
        }
    }
}