using System.IO;
using System.Reflection;
using UnityEditor;

namespace TableForge.Tests
{
    internal static class PathUtil
    {
        public static string GetAndCreateTestDataFolder()
        {
            string path = Editor.UI.PathUtil.GetRelativeDataPath("TestData").Replace("\\", "/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
         
            return path;
        }
    }
}