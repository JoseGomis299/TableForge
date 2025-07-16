using System;

namespace TableForge.Editor.Serialization
{
    internal static class TableDeserializerFactory
    {
        public static TableDeserializer Create(SerializationFormat format, string data, string tableName, string newElementsBasePath, string newElementsBaseName, Type itemsType, bool csvHasHeader)
        {
            return format switch
            {
                SerializationFormat.Json => new JsonTableDeserializer(data, tableName, newElementsBasePath, newElementsBaseName, itemsType),
                SerializationFormat.Csv => new CsvTableDeserializer(data, tableName, newElementsBasePath, newElementsBaseName, itemsType, csvHasHeader),
                _ => throw new NotSupportedException($"Deserialization format {format} is not supported.")
            };
        }
    }
}