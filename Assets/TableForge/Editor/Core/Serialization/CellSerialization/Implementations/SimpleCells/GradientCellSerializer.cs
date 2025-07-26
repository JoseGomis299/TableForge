using UnityEngine;

namespace TableForge.Editor.Serialization
{
    internal class GradientCellSerializer : CellSerializer
    {
        public GradientCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            SerializableGradient data = new SerializableGradient((Gradient) cell.GetValue());
            return serializer.Serialize(data);
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableGradient value = serializer.Deserialize<SerializableGradient>(data);
            if (value is not null)
            {
                cell.SetValue(value.ToGradient());
            }
        }
    }
} 