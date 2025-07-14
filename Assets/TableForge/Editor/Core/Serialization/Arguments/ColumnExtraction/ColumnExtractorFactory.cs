namespace TableForge.Editor.Serialization
{
    internal static class ColumnExtractorFactory
    {
        private static JsonColumnExtractor _jsonColumnExtractor;
        private static CsvColumnExtractor _csvColumnExtractor;
        
        public static ColumnExtractor CreateExtractor(TableDeserializationArgs args)
        {
            return args.Format switch
            {
                SerializationFormat.Json => _jsonColumnExtractor ??= new JsonColumnExtractor(),
                SerializationFormat.Csv => _csvColumnExtractor ??= new CsvColumnExtractor(),
                _ => null
            };
        }
    }
}