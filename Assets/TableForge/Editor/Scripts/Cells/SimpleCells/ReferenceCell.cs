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
        private Object _object;
        private string _guid;
        private string _path;
        
        public ReferenceCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            object data = GetValue();
            if (data is Object obj && obj != null)
            {
                if (obj != _object)
                {
                    _object = obj;
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

            if (_object == null) return -1;
            if (referenceCell._object == null) return 1;
            if (_object == referenceCell._object) return 0;
            
            return String.Compare(_object.name, referenceCell._object.name, StringComparison.Ordinal);
        }
    }
}