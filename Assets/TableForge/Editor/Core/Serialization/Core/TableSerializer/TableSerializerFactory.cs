using System;

namespace TableForge.Editor.Serialization
{
    internal static class TableSerializerFactory
    {
        public static TableSerializer Create(Table table, SerializationFormat format, bool includeRowGuids, bool includeRowPaths, bool flattenSubTables)
        {
            return format switch
            {
                SerializationFormat.Json => new JsonTableSerializer(table, includeRowGuids, includeRowPaths),
                SerializationFormat.Csv => new CsvTableSerializer(table, includeRowGuids, includeRowPaths, flattenSubTables),
                _ => throw new NotSupportedException($"Serialization format {format} is not supported.")
            };
        }
    }
}