namespace TableForge.Editor.Serialization
{
    internal abstract class TableSerializer
    {
        public Table Table { get; }
        public SerializationFormat Format { get; }
        public bool IncludeRowGuids { get; }
        public bool IncludeRowPaths { get; }

        protected TableSerializer(Table table, SerializationFormat format, bool includeRowGuids, bool includeRowPaths)
        {
            Table = table;
            Format = format;
            IncludeRowGuids = includeRowGuids;
            IncludeRowPaths = includeRowPaths;
        }

        public abstract string Serialize(int maxRowCount = -1);
    }
}