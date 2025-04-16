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
    }
}