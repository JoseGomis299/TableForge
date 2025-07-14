namespace TableForge.Editor
{
    internal abstract class TableSerializationArgs
    {
        public Table Table { get; }
        public SerializationFormat Format { get; }
        public bool IncludeRowGuids { get; } 
        public bool IncludeRowPaths { get; } 
        
        protected TableSerializationArgs(Table table, SerializationFormat format, bool includeRowGuids, bool includeRowPaths)
        {
            Table = table;
            Format = format;
            IncludeRowGuids = includeRowGuids;
            IncludeRowPaths = includeRowPaths;
        }
    }
    
    internal class JsonTableSerializationArgs : TableSerializationArgs
    {
        public JsonTableSerializationArgs(Table table, bool includeRowGuids, bool includeRowPaths) 
            : base(table, SerializationFormat.Json, includeRowGuids, includeRowPaths) { }
    }
    
    internal class CsvTableSerializationArgs : TableSerializationArgs
    {
        public bool FlattenSubTables { get; } 
        
        public CsvTableSerializationArgs(Table table, bool includeRowGuids, bool includeRowPaths, bool flattenSubTables) 
            : base(table, SerializationFormat.Csv, includeRowGuids, includeRowPaths)
        {
            FlattenSubTables = flattenSubTables;
        }
    }
}