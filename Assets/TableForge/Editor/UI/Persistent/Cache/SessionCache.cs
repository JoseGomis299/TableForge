using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class SessionCache
    {
        private static SessionCacheData _sessionCacheData;
        private static string CachePath => PathUtil.GetRelativeDataPath("Cache");
        private const string CacheFileName = "SessionCache.asset";
        private static string CacheFullPath => Path.Combine(CachePath, CacheFileName);

        private static SessionCacheData GetCache()
        {
            if (_sessionCacheData != null)
                return _sessionCacheData;
            
            _sessionCacheData = AssetDatabase.LoadAssetAtPath<SessionCacheData>(CacheFullPath);
            if (_sessionCacheData == null)
            {
                if (!Directory.Exists(CachePath))
                    Directory.CreateDirectory(CachePath);
                
                _sessionCacheData = ScriptableObject.CreateInstance<SessionCacheData>();
                AssetDatabase.CreateAsset(_sessionCacheData, CacheFullPath);
                AssetDatabase.SaveAssets();
            }
            return _sessionCacheData;
        }

        private static void SaveSession()
        {
            if(!EditorUtility.IsDirty(_sessionCacheData))
                EditorUtility.SetDirty(_sessionCacheData);
            AssetDatabase.SaveAssets();
        }
        
        public static IReadOnlyList<TableMetadata> GetOpenTabs()
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return new List<TableMetadata>();

            return cacheData.openTabs.Values.Where(x => x != null).ToList();
        }
        
        public static void OpenTab(TableMetadata tableMetadata)
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return;

            cacheData.openTabs.Add(tableMetadata);
            SaveSession();
        }
        
        public static void CloseTab(TableMetadata tableMetadata)
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return;

            cacheData.openTabs.Remove(tableMetadata);
            SaveSession();
        }
    }
}