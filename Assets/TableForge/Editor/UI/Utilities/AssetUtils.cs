using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
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

            string newPath = $"{directory}/{newName}{extension}";
            string name = newName;
            
            if(newPath == path)
            {
                return newName;
            }

            while (PathUtil.TryLoadAsset(newPath, out _))
            {
                newName = $"{name} {counter++}";
                newPath = $"{directory}/{newName}{extension}";
                
                if (newPath == path)
                {
                    return newName;
                }
            }
            
            RenameAssetCommand command = new RenameAssetCommand(AssetDatabase.AssetPathToGUID(path), baseName, newName);
            UndoRedoManager.Do(command);
            string error = command.Error;
            
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to rename asset: {error}");
                return baseName;
            }
            
            return newName;
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
                UndoRedoManager.RemoveRelatedCommandsFromStack(guid);

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        public static bool DeleteAssets(IEnumerable<string> guid, Action onBeforeDelete = null)
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
                List<string> paths = new List<string>();
                foreach (var g in guid)
                {
                    string path = AssetDatabase.GUIDToAssetPath(g);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    paths.Add(path);
                    UndoRedoManager.RemoveRelatedCommandsFromStack(g);
                }
                
                AssetDatabase.DeleteAssets(paths.ToArray(), new List<string>());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }
            
            return false;
        }
    }
}