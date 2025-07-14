using TableForge.Editor.Serialization;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for AnimationCurve type fields.
    /// </summary>
    [CellType(typeof(AnimationCurve))]
    internal class AnimationCurveCell : Cell
    {
        public AnimationCurveCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new AnimationCurveCellSerializer(this);
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