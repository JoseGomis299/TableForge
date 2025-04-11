using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity LayerMask type fields.
    /// </summary>
    [CellType(typeof(LayerMask))]
    internal class LayerMaskCell : Cell
    {
        public LayerMaskCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}