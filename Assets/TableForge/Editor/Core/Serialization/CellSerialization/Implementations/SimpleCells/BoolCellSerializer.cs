namespace TableForge.Editor.Serialization
{
    internal class BoolCellSerializer : PrimitiveBasedCellSerializer<bool>
    {
        public BoolCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            return base.Serialize(options).ToLower();
        }
    }
} 