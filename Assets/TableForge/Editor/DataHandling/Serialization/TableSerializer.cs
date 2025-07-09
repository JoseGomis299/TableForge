using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor
{
    internal static class TableSerializer
    {
        public static string SerializeTable(TableSerializationArgs args)
        {
            if (args == null || args.Table == null || args.Table.Rows.Count == 0 || args.Table.Columns.Count == 0)
            {
                return string.Empty; // Return empty string if table is null or has no data
            }

            switch (args.Format)
            {
                case SerializationFormat.Json:
                    return SerializeAsJson((JsonTableSerializationArgs)args);
                case SerializationFormat.Csv:
                    return SerializeCsv((CsvTableSerializationArgs)args);
                default:
                    return string.Empty;
            }
        }
        
        public static void DeserializeTable(TableDeserializationArgs args, Table table)
        {
            for (int i = 0; i < table.OrderedRows.Count; i++)
            {
                Row row = table.OrderedRows[i];
                
                // Populate the cells with the reordered data
                for (int j = 0; j < args.ProcessedColumnData.Count; j++)
                {
                    if (args.ProcessedColumnData[j] == null) continue;
                    
                    string cellValue = args.ProcessedColumnData[j][i];
                    if (string.IsNullOrEmpty(cellValue)) continue;
                    
                    Cell cell = row.OrderedCells[j]; 
                    if(!cell.TryDeserialize(cellValue))
                    {
                        Debug.LogWarning($"Failed to deserialize cell value '{cellValue}' for cell {cell.GetGlobalPosition()}.");
                    }
                }
            }
        }

        private static void DeserializeCsv(CsvTableDeserializationArgs args)
        {
            throw new System.NotImplementedException();
        }

        private static void DeserializeAsJson(JsonTableDeserializationArgs args)
        {
            throw new System.NotImplementedException();
        }

        private static string SerializeAsJson(JsonTableSerializationArgs args)
        {
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonObjectStart);

            serializedData.Append($"\"{SerializationConstants.JsonTableNamePropertyName}\": \"").Append(args.Table.Name).Append($"\"{SerializationConstants.JsonItemSeparator}");
            serializedData.Append($"\"{SerializationConstants.JsonRootArrayName}\": ").Append(SerializationConstants.JsonArrayStart);
            IEnumerable<Cell> cells = args.Table.OrderedRows.SelectMany(row => row.OrderedCells);
            int currentRow = -1;

            foreach (var item in cells)
            {
                if (currentRow != item.row.Position)
                {
                    if (currentRow != -1)
                    {
                        serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                        serializedData.Append($"{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonItemSeparator}");
                    }

                    currentRow = item.row.Position;
                    serializedData.Append(SerializationConstants.JsonObjectStart);
                    
                    
                    if (args.IncludeRowGuids)
                        serializedData.Append($"\"{SerializationConstants.JsonGuidPropertyName}\": \"").Append(item.row.SerializedObject.RootObjectGuid).Append($"\"{SerializationConstants.JsonItemSeparator}");
                    
                    if (args.IncludeRowPaths)
                        serializedData.Append($"\"{SerializationConstants.JsonPathPropertyName}\": \"").Append(AssetDatabase.GetAssetPath(item.row.SerializedObject.RootObject)).Append($"\"{SerializationConstants.JsonItemSeparator}");
                    
                    serializedData.Append($"\"{SerializationConstants.JsonPropertiesPropertyName}\": ").Append(SerializationConstants.JsonObjectStart);
                }

                string value;
                if(item is IQuotedValueCell quotedValueCell) value = quotedValueCell.SerializeQuotedValue(true);
                else value = item.Serialize();
                serializedData.Append($"\"{item.column.Name}\"{SerializationConstants.JsonKeyValueSeparator} {value}{SerializationConstants.JsonItemSeparator}");
            }

            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                serializedData.Append(SerializationConstants.JsonObjectEnd).Append(SerializationConstants.JsonObjectEnd);
            }

            serializedData.Append(SerializationConstants.JsonArrayEnd);
            serializedData.Append(SerializationConstants.JsonObjectEnd);
            string unformattedJson = serializedData.ToString();
            JToken parsed = JToken.Parse(unformattedJson);
            return parsed.ToString(Formatting.Indented);
        }
        
        private static string SerializeCsv(CsvTableSerializationArgs args)
        {
            //Change the serialization settings
            bool serializeSubTablesAsJson = SerializationConstants.subTablesAsJson;
            SerializationConstants.csvCompatible = true; // We always quote necessary values in CSV serialization.
            SerializationConstants.subTablesAsJson = !args.FlattenSubTables; // If we are flattening sub-tables, we do not serialize them as JSON.
            SerializationConstants.columnSeparator = SerializationConstants.CsvColumnSeparator;
            SerializationConstants.rowSeparator = SerializationConstants.CsvRowSeparator;
            SerializationConstants.cancelledColumnSeparator = SerializationConstants.CsvCancelledColumnSeparator;
            SerializationConstants.cancelledRowSeparator = SerializationConstants.CsvCancelledRowSeparator;
            
            StringBuilder serializedData = new StringBuilder();
            if(args.Table == null || args.Table.Rows.Count == 0 || args.Table.Columns.Count == 0)
            {
                return string.Empty; 
            }
            
            //Write optional row guids and paths
            if(args.IncludeRowGuids)
                serializedData.Append("Guid").Append(SerializationConstants.CsvColumnSeparator);
            
            if(args.IncludeRowPaths)
                serializedData.Append("Path").Append(SerializationConstants.CsvColumnSeparator);
            
            //Retrieve and write the column names
            List<string> columnNames = args.FlattenSubTables ? args.Table.GetFlatteredColumnNames() : args.Table.OrderedColumns.Select(c => c.Name).ToList();
            foreach (var column in columnNames)
            {
                serializedData.Append(column).Append(SerializationConstants.CsvColumnSeparator);
            }
            
            // Remove the last column separator and add a newline
            serializedData.Remove(serializedData.Length - SerializationConstants.CsvColumnSeparator.Length, SerializationConstants.CsvColumnSeparator.Length);
            serializedData.Append(SerializationConstants.rowSeparator);
            
            // Write the data rows
            foreach (var row in args.Table.OrderedRows)
            {
                if (args.IncludeRowGuids)
                    serializedData.Append(row.SerializedObject.RootObjectGuid).Append(SerializationConstants.CsvColumnSeparator);
                
                if (args.IncludeRowPaths)
                    serializedData.Append(AssetDatabase.GetAssetPath(row.SerializedObject.RootObject)).Append(SerializationConstants.CsvColumnSeparator);
                
                foreach (var cell in row.OrderedCells)
                {
                    serializedData.Append(cell.SerializeCellCsvCompatible(args.FlattenSubTables)).Append(SerializationConstants.CsvColumnSeparator);
                }
                
                // Remove the last column separator and add a newline
                serializedData.Remove(serializedData.Length - SerializationConstants.CsvColumnSeparator.Length, SerializationConstants.CsvColumnSeparator.Length);
                serializedData.Append(SerializationConstants.rowSeparator);
            }
            
            // Remove the last row separator if it exists
            if (serializedData.Length > 0 && serializedData[^1] == SerializationConstants.rowSeparator[0])
            {
                serializedData.Remove(serializedData.Length - SerializationConstants.rowSeparator.Length, SerializationConstants.rowSeparator.Length);
            }
            
            // Restore the original setting
            SerializationConstants.subTablesAsJson = serializeSubTablesAsJson;
            SerializationConstants.csvCompatible = false;
            SerializationConstants.columnSeparator = SerializationConstants.DefaultColumnSeparator;
            SerializationConstants.rowSeparator = SerializationConstants.DefaultRowSeparator;
            SerializationConstants.cancelledColumnSeparator = SerializationConstants.DefaultCancelledColumnSeparator;
            SerializationConstants.cancelledRowSeparator = SerializationConstants.DefaultCancelledRowSeparator;
            
            return serializedData.ToString();
        }
    }
}