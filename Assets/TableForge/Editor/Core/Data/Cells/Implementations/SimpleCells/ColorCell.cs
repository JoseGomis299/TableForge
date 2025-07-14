using TableForge.Editor.Serialization;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity Color type fields.
    /// </summary>
    [CellType(typeof(Color))]
    internal class ColorCell : Cell
    {
        public ColorCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new ColorCellSerializer(this);
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not ColorCell) return 1;

            Color thisColor = (Color)GetValue();
            Color otherColor = (Color)other.GetValue();

           return thisColor.r.CompareTo(otherColor.r) != 0 ? thisColor.r.CompareTo(otherColor.r) :
                   thisColor.g.CompareTo(otherColor.g) != 0 ? thisColor.g.CompareTo(otherColor.g) :
                   thisColor.b.CompareTo(otherColor.b);
        }
    }
}