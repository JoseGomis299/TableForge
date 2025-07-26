using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableForge.Editor.Serialization;
using TableForge.Editor.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    internal class ImportViewModel
    {
        private TableDeserializer _deserializer;
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

            _deserializer = TableDeserializerFactory.Create(Format, Data, TableName, NewElementsBasePath, NewElementsBaseName, ItemsType, CsvHasHeader);
            
            foreach(var name in _deserializer.ColumnNames)
            {
                if(string.IsNullOrEmpty(name)) continue;
                AvailableFields.Add(name);
            }

            // Create column mappings based on the serialized type
            ColumnMappings.Add(new ColumnMapping
            {
                ColumnIndex = -1,
                OriginalName = "Guid",
                MappedField = AvailableFields.FirstOrDefault(field => field.ToLower().Equals("guid"))
            });
            
            ColumnMappings.Add(new ColumnMapping
            {
                ColumnIndex = -1,
                OriginalName = "Path",
                MappedField = AvailableFields.FirstOrDefault(field => field.ToLower().Equals("path"))
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
                    MappedField = AvailableFields.FirstOrDefault(field => field.ToLower().Equals(colName.ToLower()))
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
            
            List<string> guids = new(), paths = new();
            if(_columnMappingIndices[0] != -1)
                guids = _deserializer.ColumnData[_columnMappingIndices[0]];
            if(_columnMappingIndices[1] != -1)
                paths = _deserializer.ColumnData[_columnMappingIndices[1]];

            HashSet<string> createdPaths = new HashSet<string>();
            for (int i = 0; i < _deserializer.RowCount; i++)
            {
                string guid = i < guids.Count ? guids[i] : string.Empty;
                ImportItems.Add(new ImportItem { Guid = guid });

                if (guid != string.Empty)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath) && PathUtil.TryLoadAsset(assetPath, out var asset) && asset.GetType() == ItemsType)
                    {
                        ImportItems[i].Path = assetPath;
                        ImportItems[i].OriginalPath = assetPath;
                        ImportItems[i].ExistingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    }
                    else ImportItems[i].Guid = string.Empty; // No valid asset found for this GUID
                }
                
                if(!string.IsNullOrEmpty(ImportItems[i].Guid)) continue;
                string path = i < paths.Count ? paths[i] : string.Empty;
                bool assetExists = PathUtil.TryLoadAsset(path, out var existingAsset);
                
                if(string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)) || (assetExists && existingAsset.GetType() != ItemsType))
                {
                    path = PathUtil.GetUniquePath(
                        NewElementsBasePath, 
                        NewElementsBaseName, 
                        ".asset",
                        createdPaths
                        );
                    
                    createdPaths.Add(path);
                    assetExists = false;
                }
                
                ImportItems[i].Path = path;
                ImportItems[i].OriginalPath = path;
                
                if (assetExists)
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
            
            // Create assets
            int createdCount = 0;
            foreach (var item in ImportItems)
            {
                if (item.WillCreateNew)
                {
                    Debug.Log($"Creating new asset at {item.Path}");
                     
                    //Create the items without a valid GUID
                    var newData = ScriptableObject.CreateInstance(_deserializer.ItemsType);
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
            
            // Create and deserialize the table
            TableMetadata metadata = TableMetadataManager.CreateMetadata(ImportItems.Select(r => r.Guid).ToList(), TableName);
            Table table = TableMetadataManager.GetTable(metadata);
            _deserializer.Deserialize(table, SerializationOptionsFactory.GetOptions(SerializationFormat.Default), _columnMappingIndices[2..]); // Skip Guid and Path columns

        }

        public string GetDataInfo()
        {
            if (string.IsNullOrEmpty(Data) || _deserializer == null)
            {
                return "No data to preview.";
            }

            string value = "";
            foreach (var column in _deserializer.ColumnNames)
            {
                if (string.IsNullOrEmpty(column)) continue;
                value += $"{column}, ";
            }
            
            // Remove trailing comma and space
            if(value.Length > 2)
                value = value.Substring(0, value.Length - 2);
            
            return $"Fields found in text: {value}\n" +
                   $"Total rows: {_deserializer.RowCount}\n" +
                   $"Total columns: {_deserializer.ColumnNames.Count}";
        }
    }
}