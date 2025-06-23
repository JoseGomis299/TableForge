using System;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that is based on a primitive type. (e.g., int, float, string).
    /// </summary>
    internal abstract class PrimitiveBasedCell<TValue> : Cell
    {
        protected PrimitiveBasedCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            serializer = new SimpleSerializer();
        }
        
        public override string Serialize()
        {
            if (cachedValue is TValue typedValue)
            {
                return serializer.Serialize(typedValue);
            }
            return string.Empty;
        }

        public override void Deserialize(string data)
        {
            TValue value = serializer.Deserialize<TValue>(data);
            if (value is not null)
            {
                SetValue(value);
            }
        }

        public override int CompareTo(Cell other)
        {
            if (other is not PrimitiveBasedCell<TValue> primitiveCell) return 1;
            
            if (cachedValue is IComparable comparable)
            {
                return comparable.CompareTo(primitiveCell.cachedValue);
            }

            throw new InvalidOperationException("Cannot compare non-comparable values.");
        }
    }
}