using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Object type fields. Stores a reference to an object.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(Object))]
    internal class ReferenceCell : Cell
    {
        public ReferenceCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }
}