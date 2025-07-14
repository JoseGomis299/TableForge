using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for float values 
    /// </summary>
    [CellType(typeof(float))]
    internal class FloatCell : PrimitiveBasedCell<float>, INumericBasedCell
    {
        public FloatCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new FloatCellSerializer(this);
        }
    }
}