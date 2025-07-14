namespace TableForge.Editor
{
    internal static class SerializationConstants
    {
        public static bool modifySubTables = true;
        public static bool subTablesAsJson = true;
        public static bool csvCompatible = false; // If true, we will always quote necessary values in CSV serialization.
        
        public const string EmptyColumn = "null";

        public static string rowSeparator = "\n";
        public static string cancelledRowSeparator = "\\n";
        public static string columnSeparator = "\t";
        public static string cancelledColumnSeparator = "\\t";
        
        public const string DefaultRowSeparator = "\n";
        public const string DefaultCancelledRowSeparator = "\\n";
        public const string DefaultColumnSeparator = "\t";
        public const string DefaultCancelledColumnSeparator = "\\t";
        
        public const string CsvColumnSeparator = ",";
        public const string CsvRowSeparator = "\n";
        public const string CsvCancelledColumnSeparator = ",";
        public const string CsvCancelledRowSeparator = "\n";
        
        public const string JsonArrayStart = "[";
        public const string JsonArrayEnd = "]";
        
        public const string JsonObjectStart = "{";
        public const string JsonObjectEnd = "}";
        
        public const string JsonKeyValueSeparator = ":";
        public const string JsonItemSeparator = ",";
        public const string JsonNullValue = "null";
        
        public const string JsonPathPropertyName = "path";
        public const string JsonGuidPropertyName = "guid";
        public const string JsonPropertiesPropertyName = "properties";
        public const string JsonRootArrayName = "items";
        public const string JsonTableNamePropertyName = "name";
    }
}

