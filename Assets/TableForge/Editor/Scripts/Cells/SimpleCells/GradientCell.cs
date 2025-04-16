using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            SerializableGradient data = new SerializableGradient((Gradient) GetValue());
            return Serializer.Serialize(data);
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableGradient value = Serializer.Deserialize<SerializableGradient>(data);
            if (value is not null)
            {
                SetValue(value.ToGradient());
            }
        }
    }
}