using System;
using TableForge.Exceptions;

namespace TableForge
{
    /// <summary>
    /// Cell for floating point data types (float, double)
    /// </summary>
    [CellType(typeof(float), typeof(double))]
    internal class FloatingPointCell : Cell
    {
        public FloatingPointCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override void SetValue(object value)
        {
            if(!value.GetType().IsFloatingPointType())
                throw new InvalidCellValueException($"Data must be a floating point type, type provided: {value.GetType()}");
        
            base.SetValue(value);
            Value = Convert.ChangeType(value, Type);
        }
    }
}