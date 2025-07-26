using System.Globalization;

namespace TableForge.Editor.Serialization
{
    internal class DoubleCellSerializer : PrimitiveBasedCellSerializer<double>
    {
        public DoubleCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            return ((double)cell.GetValue()).ToString(CultureInfo.InvariantCulture);
        }
        
        public override void Deserialize(string serializedData, SerializationOptions options)
        {
            if (double.TryParse(serializedData, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                cell.SetValue(value);
            }
        }
        
    }
}