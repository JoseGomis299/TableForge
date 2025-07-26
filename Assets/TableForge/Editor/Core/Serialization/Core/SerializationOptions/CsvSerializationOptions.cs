namespace TableForge.Editor.Serialization
{
    internal class CsvSerializationOptions : SerializationOptions
    {
        public CsvSerializationOptions(bool flattenSubTables)
        {
            SubTablesAsJson = !flattenSubTables;
            CsvCompatible = true;
            RowSeparator = SerializationConstants.CsvRowSeparator;
            CancelledRowSeparator = SerializationConstants.CsvCancelledRowSeparator;
            ColumnSeparator = SerializationConstants.CsvColumnSeparator;
            CancelledColumnSeparator = SerializationConstants.CsvCancelledColumnSeparator;
        }
    }
}