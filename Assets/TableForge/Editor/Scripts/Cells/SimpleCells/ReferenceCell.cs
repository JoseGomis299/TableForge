using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Object type fields. Stores a reference to an object.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(Object))]
    internal class ReferenceCell : Cell
    {
        private Object _lastSerializedObject;
        private string _guid;
        private string _path;
        
        public ReferenceCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            object data = GetValue();
            if (data is Object obj && obj != null)
            {
                if (obj != _lastSerializedObject)
                {
                    _lastSerializedObject = obj;
                    _path = AssetDatabase.GetAssetPath(obj);
                    _guid = AssetDatabase.AssetPathToGUID(_path);
                }

                data = new SerializableObject(_guid, _path, obj);
                return Serializer.Serialize(data);
            }
            
            return "NULL";
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            if (data == "NULL") 
            {
                SetValue(null);
                return;
            }

            SerializableObject value = Serializer.Deserialize<SerializableObject>(data);
            if (value is not null)
            {
                SetValue(value.ToObject());
            }
        }

        public override int CompareTo(Cell other)
        {
            if (other is not ReferenceCell referenceCell) return 1;
            Object thisObject = Value as Object;
            Object otherObject = referenceCell.Value as Object;

            if (thisObject == null && otherObject == null) return 0;
            if (thisObject == null) return -1;
            if (otherObject == null) return 1;
            
           return String.Compare(thisObject.name, otherObject.name, StringComparison.Ordinal);
        }
    }
}