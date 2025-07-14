namespace TableForge.Editor.Serialization
{
    internal class StringCellSerializer : PrimitiveBasedCellSerializer<string>, IQuotedValueCellSerializer
    {
        public StringCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            if (cell.GetValue() is string typedValue)
            {
                return serializer.Serialize(typedValue);
            }
            return string.Empty;
        }

        public string SerializeQuotedValue(bool escapeInternalQuotes)
        { 
            string serializedValue = Serialize();
            if (escapeInternalQuotes)
            {
                serializedValue = serializedValue.Replace("\"", "\\\"");
            }
            return "\"" + serializedValue + "\"";
        }

        public override void Deserialize(string data)
        {
            string value = serializer.Deserialize<string>(data);
            if (value is not null)
            {
                cell.SetValue(value);
            }
        }
    }
} 