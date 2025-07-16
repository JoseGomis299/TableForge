using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace TableForge.Editor.Serialization
{
    internal class JsonTableDeserializer : TableDeserializer
    {
        public JsonTableDeserializer(string data, string tableName, string newElementsBasePath, string newElementsBaseName, Type itemsType) 
            : base(data, tableName, newElementsBasePath, newElementsBaseName,SerializationFormat.Json, itemsType) { }
        
        protected override List<string> ExtractColumnNames()
        {
            var columnNames = new List<string>();
            if (string.IsNullOrEmpty(Data)) return columnNames;

            var jsonObject = JObject.Parse(Data);
            var rows = jsonObject[SerializationConstants.JsonRootArrayName] as JArray;
            if (rows == null || rows.Count == 0) return columnNames;

            if (rows[0] is not JObject row) return columnNames;
            if (row[SerializationConstants.JsonPropertiesPropertyName] is not JObject properties) return columnNames;

            if (row.Properties().Any(p => p.Name == SerializationConstants.JsonGuidPropertyName)) 
                columnNames.Add(SerializationConstants.JsonGuidPropertyName);
            
            if (row.Properties().Any(p => p.Name == SerializationConstants.JsonPathPropertyName))
                columnNames.Add(SerializationConstants.JsonPathPropertyName);

            foreach (var property in properties.Properties())
            {
                columnNames.Add(property.Name);
            }

            return columnNames;
        }

        protected override List<List<string>> ExtractColumnData()
        {
            var columnData = new List<List<string>>();
            if (string.IsNullOrEmpty(Data)) return columnData;

            var jsonObject = JObject.Parse(Data);
            var rows = jsonObject[SerializationConstants.JsonRootArrayName] as JArray;
            if (rows == null || rows.Count == 0) return columnData;

            foreach (var rowToke in rows)
            {
                if (rowToke is not JObject row) continue;
                var properties = row[SerializationConstants.JsonPropertiesPropertyName] as JObject;
                if (properties == null || properties.Count == 0) continue;
                
                int i = 0;
                if (columnData.Count <= i)
                    columnData.Add(new List<string>());
                    
                bool hasGuid = row.Properties().Any(p => p.Name == SerializationConstants.JsonGuidPropertyName);
                var guidValue = hasGuid ? row[SerializationConstants.JsonGuidPropertyName]?.ToString() ?? string.Empty : string.Empty;
                columnData[i].Add(guidValue);
                i++;
                
                if (columnData.Count <= i)
                    columnData.Add(new List<string>());
                
                bool hasPath = row.Properties().Any(p => p.Name == SerializationConstants.JsonPathPropertyName);
                var nameValue = hasPath ? row[SerializationConstants.JsonPathPropertyName]?.ToString() ?? string.Empty : string.Empty;
                columnData[i].Add(nameValue);
                i++;
                
                foreach (var prop in properties.Properties())
                {
                    if (columnData.Count <= i)
                        columnData.Add(new List<string>());

                    var value = prop.Value?.ToString() ?? string.Empty;
                    columnData[i].Add(value);
                    i++;
                }
            }

            return columnData;
        }
    }
}