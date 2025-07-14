using UnityEngine;

namespace TableForge.Editor.Serialization
{
    internal class ColorCellSerializer : CellSerializer
    {
        public ColorCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            SerializableColor data = new SerializableColor((Color) cell.GetValue());
            return serializer.Serialize(data);
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableColor value = serializer.Deserialize<SerializableColor>(data);
            if (value is not null)
            {
                cell.SetValue(value.ToColor());
            }
        }
    }
} 