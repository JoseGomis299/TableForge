using TableForge.Editor.Serialization;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity LayerMask type fields.
    /// </summary>
    [CellType(typeof(LayerMask))]
    internal class LayerMaskCell : Cell
    {
        public LayerMaskCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new LayerMaskCellSerializer(this);
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not LayerMaskCell) return 1;
            
            LayerMask thisMask = (LayerMask)GetValue();
            LayerMask otherMask = (LayerMask)other.GetValue();
            
            // Compare the value of the masks
            return thisMask.value.CompareTo(otherMask.value);
        }
    }
}