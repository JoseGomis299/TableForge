using System.Linq;
using UnityEngine;

namespace TableForge.Editor.Serialization
{
    internal class LayerMaskCellSerializer : CellSerializer, IQuotedValueCellSerializer
    {
        public LayerMaskCellSerializer(Cell cell) : base(cell)
        {
        }

        public override string Serialize(SerializationOptions options)
        {
            return ((LayerMask)cell.GetValue()).ResolveName();
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
                cell.SetValue(new LayerMask {value = int.MaxValue});
                return;
            }
            
            if(data == "Nothing")
            {
                cell.SetValue(new LayerMask {value = 0});
                return;
            }
            
            LayerMask value = LayerMask.GetMask(data.Split(",").Select(x => x.Trim()).ToArray());
            cell.SetValue(value);
        }
    }
} 