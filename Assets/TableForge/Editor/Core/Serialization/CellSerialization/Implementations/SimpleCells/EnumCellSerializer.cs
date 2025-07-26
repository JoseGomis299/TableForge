using System;

namespace TableForge.Editor.Serialization
{
    internal class EnumCellSerializer : CellSerializer, IQuotedValueCellSerializer
    {
        public EnumCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            return cell.Type.ResolveFlaggedEnumName((int)cell.GetValue(), false);
        }

        public string SerializeQuotedValue(SerializationOptions options, bool escapeInternalQuotes)
        { 
            string serializedValue = Serialize(options);
            if (escapeInternalQuotes)
            {
                serializedValue = serializedValue.Replace("\"", "\\\"");
            }
            return "\"" + serializedValue + "\"";
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            if(data == "Everything")
            {
                cell.SetValue(-1);
                return;
            }
            
            if(data == "Nothing")
            {
                cell.SetValue(0);
                return;
            }
            
            cell.SetValue(Enum.Parse(cell.Type, data));
        }
    }
} 