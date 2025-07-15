using System.IO;
using System.Reflection;
using UnityEditor;

namespace TableForge.Tests
{
    internal static class PathUtil
    {
        public static string GetTestFolderRelativePath()
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
    }
}