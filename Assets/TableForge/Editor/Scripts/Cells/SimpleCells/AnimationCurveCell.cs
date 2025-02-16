using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for AnimationCurve type fields.
    /// </summary>
    [CellType(typeof(AnimationCurve))]
    internal class AnimationCurveCell : Cell
    {
        public AnimationCurveCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}