using System.Globalization;

namespace TableForge.Editor.Serialization
{
    internal class DoubleCellSerializer : PrimitiveBasedCellSerializer<double>
    {
        public DoubleCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            return ((double)cell.GetValue()).ToString(CultureInfo.InvariantCulture);
        }
        
        public override void Deserialize(string serializedData)
        {
            if (double.TryParse(serializedData, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                cell.SetValue(value);
            }
        }
        
    }
}