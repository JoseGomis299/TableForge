using System.IO;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal static class TableSettings
    {
        public const float MinPollingInterval = 0.5f;
        private static TableSettingsData _settingsData;
        private static string SettingsPath => PathUtil.GetRelativeDataPath("Settings");
        private const string SettingsFileName = "TableSettings.asset";
        private static string SettingsFullPath => Path.Combine(SettingsPath, SettingsFileName);

        public static TableSettingsData GetSettings()
        {
            if (_settingsData != null)
            {
                if(!EditorUtility.IsDirty(_settingsData))
                    EditorUtility.SetDirty(_settingsData);
                
                return _settingsData;
            }

            
            _settingsData = AssetDatabase.LoadAssetAtPath<TableSettingsData>(SettingsFullPath);
            if (_settingsData == null)
            {
                if (!Directory.Exists(SettingsPath))
                    Directory.CreateDirectory(SettingsPath);
                
                _settingsData = ScriptableObject.CreateInstance<TableSettingsData>();
                AssetDatabase.CreateAsset(_settingsData, SettingsFullPath);
                AssetDatabase.SaveAssets();
            }
            
            if(!EditorUtility.IsDirty(_settingsData))
                EditorUtility.SetDirty(_settingsData);
            return _settingsData;
        }
    }
}