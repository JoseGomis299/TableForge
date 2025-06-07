using System;

namespace TableForge
{
    /// <summary>
    /// Cell for boolean type fields.
    /// </summary>
    [CellType(typeof(bool))]
    internal class BoolCell : PrimitiveBasedCell<bool>
    {
        public BoolCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            return base.Serialize().ToLower();
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not BoolCell boolCell) return 1; 
            if (Value == boolCell.Value) return String.Compare(Row.Name, other.Row.Name, StringComparison.Ordinal);
            return (bool) Value ? 1 : -1; // True is greater than False
        }
    }
}