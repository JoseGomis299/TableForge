using UnityEngine;

namespace TableForge.Editor.Serialization
{
    internal class AnimationCurveCellSerializer : CellSerializer
    {
        public AnimationCurveCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize()
        {
            object data = new SerializableCurve((AnimationCurve) cell.GetValue());
            return serializer.Serialize(data);
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableCurve serializableCurve = serializer.Deserialize<SerializableCurve>(data);
            if (serializableCurve != null)
            {
                cell.SetValue(serializableCurve.ToAnimationCurve());
            }
        }
    }
} 