using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Color type fields.
    /// </summary>
    [CellType(typeof(Color))]
    internal class ColorCell : Cell
    {
        public ColorCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }
    
        public override void SerializeData()
        {
        
        }
    }
}