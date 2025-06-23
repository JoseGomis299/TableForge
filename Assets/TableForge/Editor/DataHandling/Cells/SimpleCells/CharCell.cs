using TableForge.Editor;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Unity.VisualScripting.YamlDotNet.Serialization;

namespace TableForge
{
    [CellType(typeof(char))]
    internal class CharCell : PrimitiveBasedCell<char>, IQuotedValueCell, INumericBasedCell
    {
        public CharCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            if (Value is char typedValue && typedValue != '\0')
            {
                return "\'" + Serializer.Serialize(typedValue) + "\'";
            }
            return "\'\'";
        }
        
        public string SerializeQuotedValue()
        {
            if (Value is char typedValue && typedValue != '\0')
            {
                return "\"" + Serializer.Serialize(typedValue) + "\"";
            }
            return "\"\"";
        }
        
        public override void Deserialize(string data)
        {
            data = data.Trim('\'');
            
            if (string.IsNullOrEmpty(data))
            {
                SetValue('\0');
                return;
            }
            
            char value = Serializer.Deserialize<char>(data);
            SetValue(value);
        }
    }
}