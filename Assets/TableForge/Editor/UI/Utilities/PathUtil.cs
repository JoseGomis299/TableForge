using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class PathUtil
    {
        public static string GetRelativeDataPath(string path)
        {
            return GetPath("Data", path);
        }
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

        public static string GetPath(params string[] subPaths)
        {
            string path = GetPathToAssembly();
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = subPaths.Aggregate(path, Path.Combine);
            return path;
        }
        
        public static string GetUniquePath(string baseFolder, string baseName, string extension, List<string> invalidPaths = null)
        {
            if(!extension.StartsWith(".")) extension = $".{extension}";
            if(!baseFolder.EndsWith("/")) baseFolder += "/";
            
            string newPath = $"{baseFolder}{baseName}{extension}";
            int counter = 0;

            while (AssetPathExists(newPath) || (invalidPaths != null && invalidPaths.Contains(newPath)))
            {
                if (counter == 0)
                {
                    newPath = $"{baseFolder}{baseName}{extension}";
                    counter++;
                    continue;
                }
                
                newPath = $"{baseFolder}{baseName} {counter++}{extension}";
            }

            return newPath;
        }

        public static bool IsValidPath(string path, string expectedExtension = null)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)))
                return false;

            if (expectedExtension != null && !path.EndsWith(expectedExtension))
                return false;

            return true;
        }
        
        public static bool AssetPathExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return obj != null;
        }
    }
}