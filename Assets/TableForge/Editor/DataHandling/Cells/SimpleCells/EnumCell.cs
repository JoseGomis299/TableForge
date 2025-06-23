using System;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Enum fields.
    /// </summary>
    internal class EnumCell : Cell, IQuotedValueCell, INumericBasedCell
    {
        public EnumCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            return GetFieldType().ResolveFlaggedEnumName((int)GetValue(), false);
        }
        
        public string SerializeQuotedValue()
        { 
            return "\"" + Serialize() + "\"";
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            if(data == "Everything")
            {
                SetValue(-1);
                return;
            }
            
            if(data == "Nothing")
            {
                SetValue(0);
                return;
            }
            
            SetValue(Enum.Parse(GetFieldType(), data));
        }

        public override int CompareTo(Cell other)
        {
            if (other is not EnumCell) return 1;
            return ((int)GetValue()).CompareTo((int)other.GetValue());
        }
    }
}