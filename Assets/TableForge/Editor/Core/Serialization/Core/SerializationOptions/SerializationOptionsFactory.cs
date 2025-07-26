namespace TableForge.Editor.Serialization
{
    internal class SerializationOptionsFactory
    {
        private static SerializationOptions _defaultOptions;
        private static CsvSerializationOptions _csvOptions;
        private static JsonSerializationOptions _jsonOptions;

        public static SerializationOptions GetOptions(SerializationFormat format, bool flattenSubTables = false)
        {
            switch (format)
            {
                case SerializationFormat.Csv:
                    if (_csvOptions != null)
                    {
                        _csvOptions.SubTablesAsJson = !flattenSubTables;
                        return _csvOptions;
                    }
                    return _csvOptions ??= new CsvSerializationOptions(flattenSubTables);
                case SerializationFormat.Json:
                    return _jsonOptions ??= new JsonSerializationOptions();
                default:
                    return _defaultOptions ??= new SerializationOptions();
            }
        }
    }
}