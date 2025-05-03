using UnityEditor;
using UnityEngine;

namespace TableForge.UI
{
    internal static class ScriptableObjectExtension
    {
        public static string GetPath(this ScriptableObject scriptableObject)
        {
            string path = AssetDatabase.GetAssetPath(scriptableObject);
            return path;
        }
        
        public static string GetGUID(this ScriptableObject scriptableObject)
        {
            string path = scriptableObject.GetPath();
            return AssetDatabase.AssetPathToGUID(path);
        }

        public static void Rename(this ScriptableObject scriptableObject, string newName)
        {
            string path = scriptableObject.GetPath();
            AssetDatabase.RenameAsset(path, newName);
            AssetDatabase.Refresh();
        }
    }
}