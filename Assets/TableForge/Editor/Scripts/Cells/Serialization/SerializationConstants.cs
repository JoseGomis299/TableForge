namespace TableForge
{
    internal static class SerializationConstants
    {
        public static bool ModifySubTables = true;
        
        public static string EmptyColumn = "\\NULL";

        public static string RowSeparator = "\n";
        public static string CancelledRowSeparator = "\\n";
        public static string ColumnSeparator = "\t";
        public static string CancelledColumnSeparator = "\\t";
        
        public static string ArrayStart = "\\Array: ";
        public static string DictionaryKeysStart = "\\Keys: ";
        public static string DictionaryValuesStart = " \\Values: ";
        
        public static string CollectionItemStart = "{";
        public static string CollectionItemEnd = "}";
        public static string CollectionItemSeparator = ", ";
        
        public static string CollectionSubItemSeparator = " || ";
    }
}