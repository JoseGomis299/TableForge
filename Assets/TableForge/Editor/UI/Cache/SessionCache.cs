using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class SessionCache
    {
        private static SessionCacheData _sessionCacheData;
        private static string CachePath => PathUtil.GetPath("UI", "Cache", "Data", "SessionCache.asset");

        private static SessionCacheData GetCache()
        {
            if (_sessionCacheData != null)
                return _sessionCacheData;
            
            _sessionCacheData = AssetDatabase.LoadAssetAtPath<SessionCacheData>(CachePath);
            if (_sessionCacheData == null)
            {
                _sessionCacheData = ScriptableObject.CreateInstance<SessionCacheData>();
                AssetDatabase.CreateAsset(_sessionCacheData, CachePath);
                AssetDatabase.SaveAssets();
            }
            return _sessionCacheData;
        }

        private static void SaveSession()
        {
            EditorUtility.SetDirty(_sessionCacheData);
            AssetDatabase.SaveAssets();
        }
        
        public static IReadOnlyList<TableMetadata> GetOpenTabs()
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return new List<TableMetadata>();

            return cacheData.OpenTabs.Values.Where(x => x != null).ToList();
        }
        
        public static void OpenTab(TableMetadata tableMetadata)
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return;

            cacheData.OpenTabs.Add(tableMetadata);
            SaveSession();
        }
        
        public static void CloseTab(TableMetadata tableMetadata)
        {
            SessionCacheData cacheData = GetCache();
            if (cacheData == null)
                return;

            cacheData.OpenTabs.Remove(tableMetadata);
            SaveSession();
        }
    }
}