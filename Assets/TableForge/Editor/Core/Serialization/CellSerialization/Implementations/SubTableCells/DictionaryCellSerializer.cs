using System.Collections.Generic;
using System.Text;

namespace TableForge.Editor.Serialization
{
    internal class DictionaryCellSerializer : CollectionCellSerializer
    {
        private const string CustomEscapedQuote = "\u200B\u200C";
        private DictionaryCell DictionaryCell => (DictionaryCell)cell;
        
        public DictionaryCellSerializer(Cell cell) : base(cell)
        {
            if (cell is not TableForge.Editor.DictionaryCell)
            {
                throw new System.ArgumentException("Cell must be of type DictionaryCell", nameof(cell));
            }
        }
        
        protected override string SerializeCollection(SerializationOptions options)
        {
            return SerializeDictionary(options, DictionaryCell.GetKeys(), DictionaryCell.GetValues());
        }

        private string SerializeDictionary(SerializationOptions options, List<Cell> keys, List<Cell> values)
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(SerializationConstants.JsonObjectStart);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i].Serializer.Serialize(options);
                string value;
                if(values[i].Serializer is IQuotedValueCellSerializer quotedValueCell) value = quotedValueCell.SerializeQuotedValue(options, true);
                else value = values[i]?.Serializer.Serialize(options) ?? SerializationConstants.JsonNullValue;
                serializedData.Append($"\"{key.Replace("\"", CustomEscapedQuote)}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}");
            }
            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1);
            }
            serializedData.Append(SerializationConstants.JsonObjectEnd); 
            return serializedData.ToString();
        }

        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            
            var dictionary = JsonUtil.ToStringDictionary(data);
            if(dictionary.Count == 0) return;
            string[] values = new string[dictionary.Count * 2];
            int i = 0;
            foreach (var kvp in dictionary)
            {
                values[i++] = kvp.Key.Replace(CustomEscapedQuote, "\""); // Handle escaped quotes;
                values[i++] = kvp.Value;
            }
            int index = 0;
            
            DeserializeSubTable(values, ref index, options);
        }
        
        protected override void DeserializeModifyingSubTable(string[] values, ref int index, SerializationOptions options)
        {
            DeserializeWithoutModifyingSubTable(values, ref index, options);
        }
    }
} 