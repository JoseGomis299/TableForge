namespace TableForge.Editor
{
    [CellType(typeof(char))]
    internal class CharCell : PrimitiveBasedCell<char>, IQuotedValueCell, INumericBasedCell
    {
        public CharCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            if (cachedValue is char typedValue && typedValue != '\0')
            {
                return "\'" + serializer.Serialize(typedValue) + "\'";
            }
            return "\'\'";
        }
        
        public string SerializeQuotedValue()
        {
            if (cachedValue is char typedValue && typedValue != '\0')
            {
                return "\"" + serializer.Serialize(typedValue) + "\"";
            }
            return "\"\"";
        }
        
        public override void Deserialize(string data)
        {
            data = data.Trim('\'');
            
            if (string.IsNullOrEmpty(data))
            {
                SetValue('\0');
                return;
            }
            
            char value = serializer.Deserialize<char>(data);
            SetValue(value);
        }
    }
}