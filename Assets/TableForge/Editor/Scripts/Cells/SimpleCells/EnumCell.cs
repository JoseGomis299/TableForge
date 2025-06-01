using System;

namespace TableForge
{
    /// <summary>
    /// Cell for Enum fields.
    /// </summary>
    internal class EnumCell : Cell
    {
        public EnumCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            return $"\'{GetFieldType().ResolveFlaggedEnumName((int)GetValue(), false)}'";
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            data = data.Trim('\'');
            
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
    }
}