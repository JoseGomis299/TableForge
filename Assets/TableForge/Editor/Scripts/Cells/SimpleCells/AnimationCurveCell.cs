using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for AnimationCurve type fields.
    /// </summary>
    [CellType(typeof(AnimationCurve))]
    internal class AnimationCurveCell : Cell
    {
        public AnimationCurveCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            object data = new SerializableCurve((AnimationCurve) GetValue());
            return Serializer.Serialize(data);
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableCurve serializableCurve = Serializer.Deserialize<SerializableCurve>(data);
            if (serializableCurve != null)
            {
                SetValue(serializableCurve.ToAnimationCurve());
            }
        }
    }
}