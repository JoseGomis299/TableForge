using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;

namespace TableForge.Editor.Serialization
{
    internal class JsonTableSerializer : TableSerializer
    {
        public JsonTableSerializer(Table table, bool includeRowGuids, bool includeRowPaths) 
            : base(table, SerializationFormat.Json, includeRowGuids, includeRowPaths) { }
        
        public override string Serialize(int maxRowCount = -1)
        {
            SerializationOptions options = SerializationOptionsFactory.GetOptions(SerializationFormat.Json);
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonObjectStart);

            serializedData.Append($"\"{SerializationConstants.JsonRootArrayName}\": ").Append(SerializationConstants.JsonArrayStart);
            IEnumerable<Cell> cells = Table.OrderedRows.SelectMany(row => row.OrderedCells);
            int currentRow = -1;

            int rowCount = maxRowCount > 0 ? maxRowCount : Table.Rows.Count;
            foreach (var item in cells)
            {
                if (currentRow != item.row.Position)
                {
                    if (rowCount <= 0) break; // Stop if we reached the max row count
                    if (currentRow != -1)
                    {
                        serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                        serializedData.Append($"{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonItemSeparator}");
                    }

                    currentRow = item.row.Position;
                    serializedData.Append(SerializationConstants.JsonObjectStart);
                    
                    
                    if (IncludeRowGuids)
                        serializedData.Append($"\"{SerializationConstants.JsonGuidPropertyName}\": \"").Append(item.row.SerializedObject.RootObjectGuid).Append($"\"{SerializationConstants.JsonItemSeparator}");
                    
                    if (IncludeRowPaths)
                        serializedData.Append($"\"{SerializationConstants.JsonPathPropertyName}\": \"").Append(AssetDatabase.GetAssetPath(item.row.SerializedObject.RootObject)).Append($"\"{SerializationConstants.JsonItemSeparator}");
                    
                    serializedData.Append($"\"{SerializationConstants.JsonPropertiesPropertyName}\": ").Append(SerializationConstants.JsonObjectStart);
                    rowCount--;
                }

                string value;
                if(item.Serializer is IQuotedValueCellSerializer quotedValueCell) value = quotedValueCell.SerializeQuotedValue(options, true);
                else value = item.Serializer.Serialize(options);
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
    }
}