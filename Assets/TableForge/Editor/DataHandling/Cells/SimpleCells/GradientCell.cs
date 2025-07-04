using System.Linq;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override void SetValue(object value)
        {
            base.SetValue(value);
            if(cachedValue != null) return;

            cachedValue = new Gradient();
        }

        public override void RefreshData()
        {
            base.RefreshData();
            if(cachedValue != null) return;

            cachedValue = new Gradient();
        }

        public override string Serialize()
        {
            SerializableGradient data = new SerializableGradient((Gradient) GetValue());
            return serializer.Serialize(data);
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableGradient value = serializer.Deserialize<SerializableGradient>(data);
            if (value is not null)
            {
                SetValue(value.ToGradient());
            }
        }
        
        public override int CompareTo(Cell otherCell)
        {
            if (otherCell is not GradientCell) return 1;

            Gradient thisGradient = (Gradient)GetValue();
            Gradient otherGradient = (Gradient)otherCell.GetValue();

            // Compare the number of color keys
            int comparison = thisGradient.colorKeys.Length.CompareTo(otherGradient.colorKeys.Length);

            if (comparison == 0)
            {
                comparison = thisGradient.colorKeys
                    .Select(k => k.color)
                    .Aggregate(0f, (acc, color) => acc + color.r + color.g + color.b)
                    .CompareTo(otherGradient.colorKeys
                        .Select(k => k.color)
                        .Aggregate(0f, (acc, color) => acc + color.r + color.g + color.b));
            }

            return comparison;
        }
    }
}