namespace TableForge
{
    /// <summary>
    /// Represents a cell that is based on a primitive type. (e.g., int, float, string).
    /// </summary>
    internal abstract class PrimitiveBasedCell<TValue> : Cell
    {
        protected PrimitiveBasedCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new SimpleSerializer();
        }
        
        public override string Serialize()
        {
            if (Value is TValue typedValue)
            {
                return Serializer.Serialize(typedValue);
            }
            return string.Empty;
        }

        public override void Deserialize(string data)
        {
            TValue value = Serializer.Deserialize<TValue>(data);
            if (value is not null)
            {
                SetValue(value);
            }
        }
    }
}