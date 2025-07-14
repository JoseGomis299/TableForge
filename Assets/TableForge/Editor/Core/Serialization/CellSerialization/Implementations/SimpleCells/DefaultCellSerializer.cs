namespace TableForge.Editor.Serialization
{
    internal class DefaultCellSerializer : CellSerializer
    {
        public DefaultCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            // Default cell does not have a value to serialize, return a placeholder
            return "null";
        }

        public override void Deserialize(string data)
        {
            //No implementation needed for default cell
        }
    }
} 