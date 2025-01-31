using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TableForge
{
    internal class ScriptableObjectSelector : ItemSelector
    {
        private readonly string[] _paths;
        private readonly Dictionary<Type, List<ITFSerializedObject>> _selectedData = new Dictionary<Type, List<ITFSerializedObject>>();

        public ScriptableObjectSelector(String[] paths)
        {
            _paths = paths;
        }

        public override List<List<ITFSerializedObject>> GetItemData()
        {
            foreach (string path in _paths)
            {
                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (scriptableObject == null)
                {
                    Debug.LogError($"Failed to load asset at path {path}");
                    continue;
                }
                
                GroupData(scriptableObject);
            }
            
            return _selectedData.Values.ToList();
        }

        private void GroupData(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null) return;
            
            if(_selectedData.ContainsKey(scriptableObject.GetType()))
                _selectedData[scriptableObject.GetType()].Add(new TFSerializedObject(scriptableObject));
            else
                _selectedData.Add(scriptableObject.GetType(), new List<ITFSerializedObject> {new TFSerializedObject(scriptableObject)});
        }
    }
}