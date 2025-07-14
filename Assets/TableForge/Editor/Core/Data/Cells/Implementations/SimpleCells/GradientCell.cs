using System.Linq;
using TableForge.Editor.Serialization;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new GradientCellSerializer(this);
        }

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