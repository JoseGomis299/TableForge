using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor
{
    internal class ImportViewModel
    {
        private TableDeserializationArgs _deserializationArgs;
        private int[] _columnMappingIndices;
        
        public string TableName { get; set; }
        public SerializationFormat Format { get; set; }
        public bool CsvHasHeader { get; set; }
        public string Data { get; set; }
        public Type ItemsType { get; set; }
        public string NewElementsBasePath { get; set; }
        public string NewElementsBaseName { get; set; }
        
        public List<ColumnMapping> ColumnMappings { get; } = new List<ColumnMapping>();
        public List<ImportItem> ImportItems { get; } = new List<ImportItem>();
        public List<string> AvailableFields { get; } = new List<string>();


        public void ProcessData()
        {
            ColumnMappings.Clear();
            ImportItems.Clear();
            AvailableFields.Clear();
            
            if(Format == SerializationFormat.Json)
            {
                _deserializationArgs = new JsonTableDeserializationArgs(Data, TableName, NewElementsBasePath, NewElementsBaseName, ItemsType);
            }
            else // CSV
            {
                _deserializationArgs = new CsvTableDeserializationArgs(Data, TableName, NewElementsBasePath, NewElementsBaseName, ItemsType, CsvHasHeader);
            }
            
            foreach(var name in _deserializationArgs.ColumnNames)
            {
                if(string.IsNullOrEmpty(name)) continue;
                AvailableFields.Add(name);
            }

            // Create column mappings based on the serialized type
            ColumnMappings.Add(new ColumnMapping
            {
                ColumnIndex = -1,
                OriginalName = "Guid",
                MappedField = AvailableFields.Contains("Guid") ? "Guid" : null
            });
            
            ColumnMappings.Add(new ColumnMapping
            {
                ColumnIndex = -1,
                OriginalName = "Path",
                MappedField = AvailableFields.Contains("Path") ? "Path" : null
            });
            
            TfSerializedType serializedType = new TfSerializedType(ItemsType, null);
            for (int i = 0; i < serializedType.Fields.Count; i++)
            {
                string colName = serializedType.Fields[i].FriendlyName;
                ColumnMappings.Add(new ColumnMapping
                {
                    ColumnIndex = i,
                    ColumnLetter = PositionUtil.ConvertToLetters(i + 1),
                    OriginalName = colName,
                    MappedField = AvailableFields.Contains(colName) ? colName : null
                });
            }
        }

        public void ApplyColumnMappings()
        {
            // Create column mapping indices
            _columnMappingIndices = new int[ColumnMappings.Count];
            for (int i = 0; i < ColumnMappings.Count; i++)
            {
                var mapping = ColumnMappings[i];
                if (string.IsNullOrEmpty(mapping.MappedField))
                {
                    _columnMappingIndices[i] = -1; // No mapping
                    continue;
                }
                
                int index = AvailableFields.IndexOf(mapping.MappedField);
                _columnMappingIndices[i] = index;
            }
        }

        public void PrepareImportItems()
        {
            ImportItems.Clear();
            int itemCount = _deserializationArgs.ColumnData.Where(c => c.Count != 0).Select(c => c.Count).Max();
            
            List<string> guids = new(), paths = new();
            if(_columnMappingIndices[0] != -1)
                guids = _deserializationArgs.ColumnData[_columnMappingIndices[0]];
            if(_columnMappingIndices[1] != -1)
                paths = _deserializationArgs.ColumnData[_columnMappingIndices[1]];

            List<string> createdPaths = new List<string>();
            for (int i = 0; i < itemCount; i++)
            {
                string guid = i < guids.Count ? guids[i] : string.Empty;
                ImportItems.Add(new ImportItem { Guid = guid });

                if (guid != string.Empty)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath) && AssetDatabase.AssetPathExists(assetPath))
                    {
                        ImportItems[i].Path = assetPath;
                        ImportItems[i].OriginalPath = assetPath;
                        ImportItems[i].ExistingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    }
                    else ImportItems[i].Guid = string.Empty; // No valid asset found for this GUID
                }
                
                if(!string.IsNullOrEmpty(ImportItems[i].Guid)) continue;
                string path = i < paths.Count ? paths[i] : string.Empty;
                if(string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)))
                {
                    path = PathUtil.GetUniquePath(
                        NewElementsBasePath, 
                        NewElementsBaseName, 
                        ".asset",
                        createdPaths
                        );
                    
                    createdPaths.Add(path);
                }
                
                ImportItems[i].Path = path;
                ImportItems[i].OriginalPath = path;

                if (AssetDatabase.AssetPathExists(path))
                {
                    ImportItems[i].ExistingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    ImportItems[i].Guid = AssetDatabase.AssetPathToGUID(path);
                }
            }

            ValidateItems();
        }

        public void ValidateItems()
        {
            // Check for duplicate paths
            var paths = new HashSet<string>();
            foreach (var item in ImportItems)
            {
                if (string.IsNullOrEmpty(item.Path))
                {
                    throw new Exception("Item path cannot be empty.");
                }
                
                if (!paths.Add(item.Path))
                {
                    throw new Exception($"Duplicate path found: {item.Path}");
                }
            }
        }

        public void FinalizeImport()
        {
            ValidateItems();
            
            if (string.IsNullOrEmpty(TableName) || TableMetadataManager.LoadMetadata(TableName) != null)
            {
                throw new Exception($"Table name '{TableName}' already exists or is invalid.");
            }
            
            // This would actually create the table and assets
            Debug.Log($"Creating table '{TableName}' with {ImportItems.Count} items");
            
            // Create assets
            int createdCount = 0;
            foreach (var item in ImportItems)
            {
                if (item.WillCreateNew)
                {
                    Debug.Log($"Creating new asset at {item.Path}");
                     
                    //Create the items without a valid GUID
                    var newData = ScriptableObject.CreateInstance(_deserializationArgs.ItemsType);
                    AssetDatabase.CreateAsset(newData, item.Path);
                    item.Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newData));
                    createdCount++;
                }
            }
            
            if (createdCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            //Reorder the data based on the provided column indices, skipping the first two columns (GUID and Path)
            var columnDataCopy = _deserializationArgs.ColumnData.ToList();
            List<List<string>> processedColumnData = new List<List<string>>();
            for (var i = 2; i < _columnMappingIndices.Length; i++)
            {
                int newIndex = _columnMappingIndices[i];
                if (newIndex == -1) continue;
                
                processedColumnData.Add(columnDataCopy[newIndex]);
            }
            
            // Create the table
            TableMetadata metadata = TableMetadataManager.CreateMetadata(ImportItems.Select(r => r.Guid).ToList(), TableName);
            Table table = TableMetadataManager.GetTable(metadata);
            
            // Deserialize the data into the table
            for (int i = 0; i < table.OrderedRows.Count; i++)
            {
                Row row = table.OrderedRows[i];
                for (int j = 0; j < processedColumnData.Count; j++)
                {
                    if (processedColumnData[j] == null) continue;
                    
                    string cellValue = processedColumnData[j][i];
                    if (string.IsNullOrEmpty(cellValue)) continue;
                    
                    Cell cell = row.OrderedCells[j]; 
                    if(!cell.TryDeserialize(cellValue))
                    {
                        Debug.LogWarning($"Failed to deserialize cell value '{cellValue}' for cell {cell.GetGlobalPosition()}.");
                    }
                }
            }
        }
    }
}