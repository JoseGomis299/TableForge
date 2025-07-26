namespace TableForge.Editor.Serialization
{
    internal class PrimitiveBasedCellSerializer<TValue> : CellSerializer
    {
        public PrimitiveBasedCellSerializer(Cell cell) : base(cell)
        {
            serializer = new SimpleSerializer();
        }

        public override string Serialize(SerializationOptions options)
        {
            if (cell.GetValue() is TValue typedValue)
            {
                return serializer.Serialize(typedValue);
            }
            return string.Empty;
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            TValue value = serializer.Deserialize<TValue>(data);
            if (value is not null)
            {
                cell.SetValue(value);
            }
        }
    }
} 