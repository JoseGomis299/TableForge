using System;

namespace TableForge.Editor.Serialization
{
    internal static class SerializationConstants
    {
        public const string EmptyColumn = "null";
        
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
    }
}

