using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class TableSettings
    {
        public const float MinPollingInterval = 0.5f;
        private static TableSettingsData _settingsData;
        private static string SettingsPath => PathUtil.GetPath("UI", "Settings", "Data", "TableSettings.asset");

        public static TableSettingsData GetSettings()
        {
            if (_settingsData != null)
            {
                if(!EditorUtility.IsDirty(_settingsData))
                    EditorUtility.SetDirty(_settingsData);
                
                return _settingsData;
            }

            _settingsData = AssetDatabase.LoadAssetAtPath<TableSettingsData>(SettingsPath);
            if (_settingsData == null)
            {
                _settingsData = ScriptableObject.CreateInstance<TableSettingsData>();
                AssetDatabase.CreateAsset(_settingsData, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            
            if(!EditorUtility.IsDirty(_settingsData))
                EditorUtility.SetDirty(_settingsData);
            return _settingsData;
        }
    }
}