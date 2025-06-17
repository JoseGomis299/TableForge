using System;

namespace TableForge
{
    /// <summary>
    /// Represents a cell that is based on a primitive type. (e.g., int, float, string).
    /// </summary>
    internal abstract class PrimitiveBasedCell<TValue> : Cell
    {
        protected PrimitiveBasedCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new SimpleSerializer();
        }
        
        public override string Serialize()
        {
            if (Value is TValue typedValue)
            {
                return Serializer.Serialize(typedValue);
            }
            return string.Empty;
        }

        public override void Deserialize(string data)
        {
            TValue value = Serializer.Deserialize<TValue>(data);
            if (value is not null)
            {
                SetValue(value);
            }
        }

        public override int CompareTo(Cell other)
        {
            if (other is not PrimitiveBasedCell<TValue> primitiveCell) return 1;
            
            if (Value is IComparable comparable)
            {
                return comparable.CompareTo(primitiveCell.Value);
            }

            throw new InvalidOperationException("Cannot compare non-comparable values.");
        }
    }
}