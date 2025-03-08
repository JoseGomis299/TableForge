using System;
using TableForge.Exceptions;

namespace TableForge
{
    /// <summary>
    /// Cell for integral type fields. (int, uint, long, ulong)
    /// </summary>
    [CellType(/*typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), */typeof(int), typeof(uint), typeof(long), typeof(ulong))]
    internal class IntegralCell : Cell
    {
        public IntegralCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override void SetValue(object value)
        {
            if(!value.GetType().IsIntegralType())
                throw new InvalidCellValueException($"Data must be an integral type, type {value.GetType().Name} is not valid for this cell!");
          
            base.SetValue(value);
        }
    }
}