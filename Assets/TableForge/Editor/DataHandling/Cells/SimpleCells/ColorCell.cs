using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity Color type fields.
    /// </summary>
    [CellType(typeof(Color))]
    internal class ColorCell : Cell
    {
        public ColorCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            SerializableColor data = new SerializableColor((Color) GetValue());
            return serializer.Serialize(data);
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableColor value = serializer.Deserialize<SerializableColor>(data);
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

           return thisColor.r.CompareTo(otherColor.r) != 0 ? thisColor.r.CompareTo(otherColor.r) :
                   thisColor.g.CompareTo(otherColor.g) != 0 ? thisColor.g.CompareTo(otherColor.g) :
                   thisColor.b.CompareTo(otherColor.b);
        }
    }
}