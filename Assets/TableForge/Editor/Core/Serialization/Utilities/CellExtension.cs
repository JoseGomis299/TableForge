namespace TableForge.Editor.Serialization
{
    internal static class CellExtension
    {
        /// <summary>
        /// Serializes a cell's value in a CSV-compatible format.
        /// </summary>
        public static string SerializeCellCsvCompatible(this Cell cell, SerializationOptions options, bool flattenSubTables)
        {
            string value;
            if (cell.Serializer is IQuotedValueCellSerializer quotedValueCell)
            {
                value = quotedValueCell.SerializeQuotedValue(options, true).Replace("\\\"", "\"\"").Replace("\'", "\"\""); // Escape quotes for CSV
            }
            else
            {
                value = cell.Serializer.Serialize(options);
                if((cell is SubTableCell && (!flattenSubTables || cell is ICollectionCell)) || cell.Serializer.ValueSerializer is JsonSerializer)
                {
                    // If we serialize the value as JSON we have to surround the value with quotes and escape quotes inside the value
                    value = $"\"{value.Replace("\"", "\"\"")}\"";
                }
            }

            return value;
        }
    }
}