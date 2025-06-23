namespace TableForge.Editor
{
    /// <summary>
    /// Cell for string type fields.
    /// </summary>
    [CellType(typeof(string))]
    internal class StringCell : PrimitiveBasedCell<string>, IQuotedValueCell
    {
        public StringCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            if (cachedValue is string typedValue)
            {
                return serializer.Serialize(typedValue);
            }
            return string.Empty;
        }
        
        public string SerializeQuotedValue()
        { 
            return "\"" + Serialize() + "\"";
        }
        
        public override void Deserialize(string data)
        {
            string value = serializer.Deserialize<string>(data);
            if (value is not null)
            {
                SetValue(value);
            }
        }
    }
}