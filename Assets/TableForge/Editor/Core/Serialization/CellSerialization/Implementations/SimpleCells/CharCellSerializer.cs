namespace TableForge.Editor.Serialization
{
    internal class CharCellSerializer : PrimitiveBasedCellSerializer<char>, IQuotedValueCellSerializer
    {
        public CharCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            if (cell.GetValue() is char typedValue && typedValue != '\0')
            {
                return "\'" + serializer.Serialize(typedValue) + "\'";
            }
            return "\'\'";
        }
    
        public string SerializeQuotedValue(bool escapeInternalQuotes)
        {
            if (cell.GetValue() is char typedValue && typedValue != '\0')
            {
                if (escapeInternalQuotes)
                {
                    typedValue = typedValue == '"' ? '\'' : typedValue;
                }
            
                return "\"" + serializer.Serialize(typedValue) + "\"";
            }
            return "\"\"";
        }
    
        public override void Deserialize(string data)
        {
            data = data.Trim('\'');
        
            if (string.IsNullOrEmpty(data))
            {
                cell.SetValue('\0');
                return;
            }
        
            char value = serializer.Deserialize<char>(data);
            cell.SetValue(value);
        }
    }
}
