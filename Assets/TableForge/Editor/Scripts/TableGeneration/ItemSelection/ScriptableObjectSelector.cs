using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableForge
{
    //WIP
    internal class ScriptableObjectSelector : ItemSelector
    {
        public override void Open()
        {
            
        }

        public override List<List<ITFSerializedObject>> GetItemData()
        {
            List<List<ITFSerializedObject>> items = new List<List<ITFSerializedObject>>();
            Dictionary<Type, List<ITFSerializedObject>> itemData = new Dictionary<Type, List<ITFSerializedObject>>();
            
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

            foreach (string guid in guids)
            {
                // Get the asset path from the GUID
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (!path.StartsWith("Assets/") || path.StartsWith("Assets/Settings"))
                    continue;
                
                // Load the asset as a ScriptableObject
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (obj != null)
                {
                    scriptableObjects.Add(obj);
                    
                    if(itemData.ContainsKey(obj.GetType()))
                        itemData[obj.GetType()].Add(new TFSerializedObject(obj));
                    else
                        itemData.Add(obj.GetType(), new List<ITFSerializedObject> {new TFSerializedObject(obj)});
                    
                    Debug.Log($"Found ScriptableObject: {obj.name} at path: {path}");
                }
            }
            
            foreach (var kvp in itemData)
            {
                items.Add(kvp.Value);
            }
            
            return items;
        }
    }
}