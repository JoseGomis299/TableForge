using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Color type fields.
    /// </summary>
    [CellType(typeof(Color))]
    internal class ColorCell : Cell
    {
        public ColorCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}