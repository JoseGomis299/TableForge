using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor
{
    internal class ScriptableObjectSelector : ItemSelector
    {
        private readonly string[] _paths;
        private readonly Dictionary<Type, List<ScriptableObject>> _preSelectedData = new();
        private readonly Dictionary<Type, List<ITfSerializedObject>> _selectedData = new();

        public ScriptableObjectSelector(String[] paths)
        {
            _paths = paths;
        }

        public override List<List<ITfSerializedObject>> GetItemData()
        {
            foreach (string path in _paths)
            {
                if (!path.EndsWith(".asset"))
                {
                    String[] guids = AssetDatabase.FindAssets("", new []{path});
                    
                    if(guids.Length == 0)
                    {
                        Debug.LogError($"Failed to load assets at path {path}");
                        continue;
                    }
                    
                    foreach (string guid in guids)
                    {
                        string p = AssetDatabase.GUIDToAssetPath(guid);     
                        GroupData(AssetDatabase.LoadAssetAtPath<ScriptableObject>(p));
                    }
                    
                    continue;
                }
                
                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (scriptableObject == null)
                {
                    Debug.LogError($"Failed to load asset at path {path}");
                    continue;
                }
                
                GroupData(scriptableObject);
            }
            
            GroupData();
            return _selectedData.Values.Count > 0 ? _selectedData.Values.ToList() : new List<List<ITfSerializedObject>> {new()};
        }

        private void GroupData(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null) return;
            
            //Get the base type of the scriptable object in case it is a derived type
            Type type = scriptableObject.GetType();
            
            if(_preSelectedData.TryGetValue(type, out var collection))
                collection.Add(scriptableObject);
            else
                _preSelectedData.Add(type, new List<ScriptableObject> {scriptableObject});
        }

        private void GroupData()
        {
            Dictionary<Type, List<Type>> baseTypesInheritors = new Dictionary<Type, List<Type>>();
            Dictionary<Type, Type> typeMapping = new Dictionary<Type, Type>();
            foreach (var kvp in _preSelectedData)
            {
                Type type = kvp.Key;
                List<ScriptableObject> items = kvp.Value;

                Type itemBaseType = items.First().GetType();
                while (itemBaseType.BaseType != null && itemBaseType.BaseType != typeof(ScriptableObject))
                {
                    itemBaseType = itemBaseType.BaseType;
                }
                
                typeMapping.TryAdd(type, itemBaseType);
                
                if (!baseTypesInheritors.TryGetValue(itemBaseType, out var types))
                {
                    types = new List<Type>();
                    baseTypesInheritors[itemBaseType] = types;
                }
                
                baseTypesInheritors[itemBaseType].Add(type);
            }
            
            HashSet<Type> typesToMap = baseTypesInheritors.Values.Where(v => v.Count > 1).SelectMany(v => v).ToHashSet();
            foreach (var kvp in _preSelectedData)
            {
                Type type = kvp.Key;
                List<ScriptableObject> items = kvp.Value;
                
                if(typesToMap.Contains(type))
                    type = typeMapping[type];

                foreach (var scriptableObject in items)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptableObject));
                    if(_selectedData.TryGetValue(type, out var collection))
                        collection.Add(new TfSerializedObject(scriptableObject, null, scriptableObject, guid, typeOverride:type));
                    else
                        _selectedData.Add(type, new List<ITfSerializedObject> {new TfSerializedObject(scriptableObject, null, scriptableObject, guid, typeOverride:type)});
                }
            }
        }
    }
}