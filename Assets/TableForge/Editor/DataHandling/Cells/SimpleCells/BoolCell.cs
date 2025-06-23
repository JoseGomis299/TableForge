using System;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for boolean type fields.
    /// </summary>
    [CellType(typeof(bool))]
    internal class BoolCell : PrimitiveBasedCell<bool>
    {
        public BoolCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override string Serialize()
        {
            return base.Serialize().ToLower();
        }
        
        public override int CompareTo(Cell other)
        {
            if (other is not BoolCell boolCell) 
                return 1;

            bool thisValue = (bool)cachedValue;
            bool otherValue = (bool)boolCell.cachedValue;

            if (thisValue == otherValue)
            {
                // Fall back to row name if both bools are the same
                return string.Compare(row?.Name, other.row?.Name, StringComparison.Ordinal);
            }

            return thisValue ? 1 : -1;
        }
    }
}