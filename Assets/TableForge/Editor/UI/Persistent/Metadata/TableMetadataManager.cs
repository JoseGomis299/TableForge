using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class TableMetadataManager
    {
        #region Metadata Management

        public static TableMetadata GetMetadata(Table table, string tableName, string basePath= null)
        {
            return LoadMetadata(tableName, basePath) ?? CreateMetadata(table, tableName, basePath);
        }
        
        public static TableMetadata GetMetadata(IEnumerable<string> itemGUIDs, string tableName, string basePath= null)
        {
            return LoadMetadata(tableName, basePath) ?? CreateMetadata(itemGUIDs, tableName, basePath);
        }
        
        public static TableMetadata CreateMetadata(IEnumerable<string> itemGUIDs, string tableName, string basePath = null)
        {
            string path = basePath ?? GetDataPath();
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var metadata = CreateMetadata(tableName);
            Type itemsType = null;
            foreach (var guid in itemGUIDs)
            {
                itemsType ??= AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid));
                metadata.AddItemGuid(guid);
            }
            metadata.SetItemsType(itemsType);
            
            StoreMetadata(tableName, path, metadata);
            return metadata;
        }
        
        public static TableMetadata CreateMetadata(Type itemsType, string tableName, string basePath = null)
        {
            string path = basePath ?? GetDataPath();
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var metadata = CreateMetadata(tableName);
            metadata.SetBindingType(itemsType);
            metadata.SetItemsType(itemsType);

            StoreMetadata(tableName, path, metadata);
            return metadata;
        }

        private static void StoreMetadata(string tableName, string path, TableMetadata metadata)
        {
            string assetPath = Path.Combine(path, tableName + ".asset");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            AssetDatabase.CreateAsset(metadata, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static TableMetadata CreateMetadata(string tableName)
        {
            TableMetadata metadata = ScriptableObject.CreateInstance<TableMetadata>();
            metadata.Name = tableName;
            metadata.IsTransposed = false;
            return metadata;
        }


        private static TableMetadata CreateMetadata(Table table, string tableName, string basePath = null)
        {
            string path = basePath ?? GetDataPath();
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            TableMetadata metadata = ScriptableObject.CreateInstance<TableMetadata>();
            metadata.Name = tableName;
            metadata.IsTransposed = false;
            metadata.SetItemGUIDs(table);

            string assetPath = Path.Combine(path, tableName + ".asset");
            AssetDatabase.CreateAsset(metadata, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return metadata;
        }

        public static TableMetadata LoadMetadata(string tableName, string basePath = null)
        {
            string path = basePath ?? GetDataPath();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return null;
            }

            string assetPath = Path.Combine(path, tableName + ".asset");
            return AssetDatabase.LoadAssetAtPath<TableMetadata>(assetPath);
        }
        
        public static List<TableMetadata> GetAllMetadata()
        {
            string path = GetDataPath();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return new List<TableMetadata>();
            }

            string[] guids = AssetDatabase.FindAssets("t:TableMetadata", new[] { path });
            List<TableMetadata> metadataList = new List<TableMetadata>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TableMetadata metadata = AssetDatabase.LoadAssetAtPath<TableMetadata>(assetPath);
                if (metadata != null)
                {
                    metadataList.Add(metadata);
                }
            }

            return metadataList;
        }

        #endregion

        #region Table Creation

        public static Table GetTable(TableMetadata metadata)
        {
            metadata.UpdateRowsPosition();
            Table table = TableManager.GenerateTable(metadata.ItemGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray(), metadata.Name);

            if (!metadata.HasAnchorData())
            {
                metadata.SetAnchorPositions(table);
            }
            else
            {
                int[] rowPositions = new int[table.Rows.Count];
                for (int i = 0; i < rowPositions.Length; i++)
                {
                    int anchorPosition = metadata.GetAnchorPosition(table.Rows[i + 1].Id) - 1;
                    rowPositions[anchorPosition] = i + 1;
                }
                
                table.SetRowOrder(rowPositions);
            }
            
            return table;
        }

        #endregion

        #region Path Getters
        
        private static string GetDataPath() => PathUtil.GetRelativeDataPath("Metadata");

        #endregion
    }
}