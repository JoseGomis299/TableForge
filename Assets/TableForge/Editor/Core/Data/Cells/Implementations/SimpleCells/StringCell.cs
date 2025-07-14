using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for string type fields.
    /// </summary>
    [CellType(typeof(string))]
    internal class StringCell : PrimitiveBasedCell<string>
    {
        public StringCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new StringCellSerializer(this);
        }
    }
}