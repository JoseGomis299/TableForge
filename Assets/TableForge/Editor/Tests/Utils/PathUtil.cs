using System.IO;
using UnityEditor;

namespace TableForge.Tests
{
    internal static class PathUtil
    {
        private const string RELATIVE_PATH = "/TableForge/Editor/Tests/";
        
        public static string GetTestFolderRelativePath()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(RELATIVE_PATH))
                {
                    return Path.GetDirectoryName(path)?.Replace("\\", "/");
                }
            }
            return string.Empty;
        }
    }
}