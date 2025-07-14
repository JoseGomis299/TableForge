using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for double values
    /// </summary>
    [CellType(typeof(double))]
    internal class DoubleCell : PrimitiveBasedCell<double>, INumericBasedCell
    {
        public DoubleCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new DoubleCellSerializer(this);
        }
    }
}