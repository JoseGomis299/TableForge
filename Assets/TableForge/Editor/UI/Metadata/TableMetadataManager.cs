using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal static class TableMetadataManager
    {
        #region Metadata Management

        public static TableMetadata GetMetadata(Table table, string tableName)
        {
            return LoadMetadata(tableName) ?? CreateMetadata(table, tableName);
        }

        private static TableMetadata CreateMetadata(Table table, string tableName)
        {
            string path = GetDataPath();
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            TableMetadata metadata = ScriptableObject.CreateInstance<TableMetadata>();
            metadata.Name = tableName;
            metadata.IsInverted = false;

            string assetPath = Path.Combine(path, tableName + ".asset");
            AssetDatabase.CreateAsset(metadata, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return metadata;
        }

        private static TableMetadata LoadMetadata(string tableName)
        {
            string path = GetDataPath();
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string assetPath = Path.Combine(path, tableName + ".asset");
            return AssetDatabase.LoadAssetAtPath<TableMetadata>(assetPath);
        }

        #endregion

        #region Path Getters

        private static string GetPathToAssembly()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(assembly.GetName().Name + ".asmdef"))
                {
                    return Path.GetDirectoryName(path)?.Replace("\\", "/");
                }
            }

            return string.Empty;
        }

        private static string GetDataPath()
        {
            string path = GetPathToAssembly();
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return Path.Combine(path, "UI", "Metadata", "Data");
        }

        #endregion
    }
}