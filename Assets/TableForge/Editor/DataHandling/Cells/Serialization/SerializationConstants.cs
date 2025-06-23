namespace TableForge.Editor
{
    internal static class SerializationConstants
    {
        public static bool modifySubTables = true;
        public static bool subTablesAsJson = true;
        
        public const string EmptyColumn = "\\NULL";

        public const string RowSeparator = "\n";
        public const string CancelledRowSeparator = "\\n";
        public const string ColumnSeparator = "\t";
        public const string CancelledColumnSeparator = "\\t";
        
        public const string JsonArrayStart = "[";
        public const string JsonArrayEnd = "]";
        
        public const string JsonObjectStart = "{";
        public const string JsonObjectEnd = "}";
        
        public const string JsonKeyValueSeparator = ":";
        public const string JsonItemSeparator = ",";
        public const string JsonNullValue = "null";
    }
}