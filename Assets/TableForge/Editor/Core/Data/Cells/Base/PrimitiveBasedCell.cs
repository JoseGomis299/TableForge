using System;
using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that is based on a primitive type. (e.g., int, float, string).
    /// </summary>
    internal abstract class PrimitiveBasedCell<TValue> : Cell
    {
        protected PrimitiveBasedCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new PrimitiveBasedCellSerializer<TValue>(this);
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