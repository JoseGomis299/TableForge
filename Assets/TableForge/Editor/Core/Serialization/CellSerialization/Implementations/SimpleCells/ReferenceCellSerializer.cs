using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge.Editor.Serialization
{
    internal class ReferenceCellSerializer : CellSerializer
    {
        private Object _lastSerializedObject;
        private string _guid;
        private string _path;
        
        public ReferenceCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            object data = cell.GetValue();
            if (data is Object obj && obj != null)
            {
                if (obj != _lastSerializedObject)
                {
                    _lastSerializedObject = obj;
                    _path = AssetDatabase.GetAssetPath(obj);
                    _guid = AssetDatabase.AssetPathToGUID(_path);
                }

                data = new SerializableObject(_guid, _path, obj);
                return serializer.Serialize(data);
            }
            
            return "null";
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            if (data == "null") 
            {
                cell.SetValue(null);
                return;
            }

            SerializableObject value = serializer.Deserialize<SerializableObject>(data);
            if (value is not null)
            {
                cell.SetValue(value.ToObject());
            }
        }
    }
} 