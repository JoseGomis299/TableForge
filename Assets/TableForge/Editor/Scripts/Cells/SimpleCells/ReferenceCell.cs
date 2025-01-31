using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Object type fields. Stores a reference to an object.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(Object))]
    internal class ReferenceCell : Cell
    {
        public ReferenceCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }
        
        public override void SerializeData()
        {
        
        }
    }
}