namespace TableForge.Editor
{
    /// <summary>
    /// Fallback cell type for unsupported data types.
    /// </summary>
    internal class DefaultCell : Cell
    {
        public DefaultCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            return "NULL";
        }
        
        public override void Deserialize(string data)
        {
            // No implementation needed for default cell
        }
        
        public override int CompareTo(Cell other)
        {
            return -1;
        }
    }
}