using System;

namespace TableForge
{
    /// <summary>
    /// Cell for floating point data types (float, double)
    /// </summary>
    [CellType(typeof(float), typeof(double))]
    internal class FloatingPointCell : Cell
    {
        public FloatingPointCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }

        public override void SetValue(object value)
        {
            if(!value.GetType().IsFloatingPointType())
                throw new ArgumentException($"Data must be a floating point type, type provided: {value.GetType()}");
        
            base.SetValue(value);
            Value = Convert.ChangeType(value, Type);
        }

        public override void SerializeData()
        {
            
        }
    }
}