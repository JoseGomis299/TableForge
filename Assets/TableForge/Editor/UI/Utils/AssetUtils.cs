using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge.UI
{
    internal static class AssetUtils
    {
        public static string RenameAsset(string path, string newName)
        {
            string directory = path.Substring(0, path.LastIndexOf('/'));
            string extension = path.Substring(path.LastIndexOf('.'));
            string baseName = path.Substring(path.LastIndexOf('/') + 1);
            baseName = baseName.Substring(0, baseName.Length - extension.Length);
            int counter = 1;

            string name = newName;
            string newPath = $"{directory}/{name}{extension}";
            
            if(newPath == path)
            {
                return name;
            }

            while (AssetDatabase.AssetPathExists(newPath))
            {
                name = $"{newName} {counter++}";
                newPath = $"{directory}/{name}{extension}";
            }
            
            string error = AssetDatabase.RenameAsset(path, name);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to rename asset: {error}");
                name = baseName;
            }
            
            return name;
        }
        
        public static bool DeleteAsset(string guid, Action onBeforeDelete = null)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return false;

            string assetName = path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - path.LastIndexOf('/') - 1);
            bool confirmed = EditorUtility.DisplayDialog(
                "Confirm Action",
                $"Are you sure you want to delete the selected asset? This action cannot be undone. ({assetName})",
                "Yes",
                "No"
            );

            if (confirmed)
            {
                onBeforeDelete?.Invoke();
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        public static bool DeleteAssets(IEnumerable<string> guid, Action onBeforeDelete = null, Action onBeforeDeleteEach = null)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Confirm Action",
                $"Are you sure you want to delete the selected assets? This action cannot be undone. (multiple assets selected)",
                "Yes",
                "No"
            );
            
            if (confirmed)
            {
                onBeforeDelete?.Invoke();
                foreach (var g in guid)
                {
                    string path = AssetDatabase.GUIDToAssetPath(g);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    onBeforeDeleteEach?.Invoke();
                    AssetDatabase.DeleteAsset(path);
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }
            
            return false;
        }
    }
}