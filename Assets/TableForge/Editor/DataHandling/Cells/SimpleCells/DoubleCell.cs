using System.Globalization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for double values
    /// </summary>
    [CellType(typeof(double))]
    internal class DoubleCell : PrimitiveBasedCell<double>, INumericBasedCell
    {
        public DoubleCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            return ((double)GetValue()).ToString(CultureInfo.InvariantCulture);
        }
        
        public override void Deserialize(string serializedData)
        {
            if (double.TryParse(serializedData, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                SetValue(value);
            }
        }
    }
}