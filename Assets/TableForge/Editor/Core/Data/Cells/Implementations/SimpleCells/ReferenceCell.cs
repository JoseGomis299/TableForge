using System;
using TableForge.Editor.Serialization;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity Object type fields. Stores a reference to an object.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(Object))]
    internal class ReferenceCell : Cell
    {
        public ReferenceCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new ReferenceCellSerializer(this);
        }

        public override int CompareTo(Cell other)
        {
            if (other is not ReferenceCell referenceCell) return 1;
            Object thisObject = cachedValue as Object;
            Object otherObject = referenceCell.cachedValue as Object;

            if (thisObject == null && otherObject == null) return 0;
            if (thisObject == null) return -1;
            if (otherObject == null) return 1;
            
           return String.Compare(thisObject.name, otherObject.name, StringComparison.Ordinal);
        }
    }
}