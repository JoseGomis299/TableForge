using System;

namespace TableForge
{
    /// <summary>
    /// Cell for integral type fields. (int, uint, long, ulong)
    /// </summary>
    [CellType(typeof(int), typeof(uint), typeof(long), typeof(ulong))]
    internal class IntegralCell : Cell
    {
        public IntegralCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }

        public override void SetValue(object value)
        {
            if(!value.GetType().IsIntegralType())
                throw new ArgumentException($"Data must be an integral type, type {value.GetType().Name} is not valid for this cell!");
          
            base.SetValue(value);
            Value = Convert.ChangeType(value, Type);
        }

        public override void SerializeData()
        {
        }
    }
}