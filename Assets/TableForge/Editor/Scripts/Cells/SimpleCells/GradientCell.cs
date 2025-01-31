using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }

        public override void SerializeData()
        {
        
        }
    }
}