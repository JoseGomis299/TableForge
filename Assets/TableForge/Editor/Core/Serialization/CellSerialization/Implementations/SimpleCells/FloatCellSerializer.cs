using System.Globalization;

namespace TableForge.Editor.Serialization
{
    internal class FloatCellSerializer : PrimitiveBasedCellSerializer<float>
    {
        public FloatCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            return ((float)cell.GetValue()).ToString(CultureInfo.InvariantCulture);
        }

        public override void Deserialize(string serializedData, SerializationOptions options)
        {
            if (float.TryParse(serializedData, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                cell.SetValue(value);
            }
        }
    }
} 