using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}