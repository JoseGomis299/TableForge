using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace TableForge.UI
{
    internal static class PathUtil
    {
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
    }
}