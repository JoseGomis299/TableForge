using UnityEditor;
using UnityEngine;

namespace TableForge
{
    [System.Serializable]
    internal class SerializableObject
    {
        public string name;
        public string path;
        public string guid;
        public int instanceID;
        
        public SerializableObject(string guid, string path, Object obj)
        {
            this.guid = guid;
            this.name = obj.name;
            this.instanceID = obj.GetInstanceID();
        }
        
        public Object ToObject()
        {
            return EditorUtility.InstanceIDToObject(instanceID) ??
                   AssetDatabase.LoadAssetAtPath<Object>(path) ??
                   AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}