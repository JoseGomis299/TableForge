namespace TableForge
{
    internal static class SerializationConstants
    {
        public static bool ModifySubTables = true;
        public static bool SubTablesAsJson = true;
        
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

        public const string JsonStart = "\u200B{"; //This is added to be able to distinguish subTables serialized as JSON from 
                                                  //Other type of items serialized as JSON. (ReferenceCell, ColorCell, GradientCell and AnimationCurveCell)
    }
}