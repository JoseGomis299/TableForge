using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for AnimationCurve type fields.
    /// </summary>
    [CellType(typeof(AnimationCurve))]
    internal class AnimationCurveCell : Cell
    {
        public AnimationCurveCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            object data = new SerializableCurve((AnimationCurve) GetValue());
            return serializer.Serialize(data);
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableCurve serializableCurve = serializer.Deserialize<SerializableCurve>(data);
            if (serializableCurve != null)
            {
                SetValue(serializableCurve.ToAnimationCurve());
            }
        }

        public override int CompareTo(Cell otherCell)
        {
            if (otherCell is not AnimationCurveCell) return 1;
            
            AnimationCurve thisCurve = (AnimationCurve)GetValue();
            AnimationCurve otherCurve = (AnimationCurve)otherCell.GetValue();

            // Compare the length of the curves
            return thisCurve.length.CompareTo(otherCurve.length);
        }
    }
}