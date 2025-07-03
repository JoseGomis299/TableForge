using System.Globalization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for float values 
    /// </summary>
    [CellType(typeof(float))]
    internal class FloatCell : PrimitiveBasedCell<float>, INumericBasedCell
    {
        public FloatCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            return ((float)GetValue()).ToString(CultureInfo.InvariantCulture);
        }
        
        public override void Deserialize(string serializedData)
        {
            if (float.TryParse(serializedData, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                SetValue(value);
            }
        }
    }
}