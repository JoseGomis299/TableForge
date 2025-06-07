using System;
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
        
        public override string Serialize()
        {
            SerializableColor data = new SerializableColor((Color) GetValue());
            return Serializer.Serialize(data);
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableColor value = Serializer.Deserialize<SerializableColor>(data);
            if (value is not null)
            {
                SetValue(value.ToColor());
            }
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not ColorCell) return 1;

            Color thisColor = (Color)GetValue();
            Color otherColor = (Color)other.GetValue();

            int comparison = thisColor.r.CompareTo(otherColor.r) != 0 ? thisColor.r.CompareTo(otherColor.r) :
                   thisColor.g.CompareTo(otherColor.g) != 0 ? thisColor.g.CompareTo(otherColor.g) :
                   thisColor.b.CompareTo(otherColor.b);
            
            if(comparison == 0)
                comparison = String.Compare(Row.Name, other.Row.Name, StringComparison.Ordinal);
            
            return comparison;
        }
    }
}