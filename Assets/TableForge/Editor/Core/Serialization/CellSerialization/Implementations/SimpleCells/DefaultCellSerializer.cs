namespace TableForge.Editor.Serialization
{
    internal class DefaultCellSerializer : CellSerializer
    {
        public DefaultCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            // Default cell does not have a value to serialize, return a placeholder
            return "null";
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            //No implementation needed for default cell
        }
    }
} 