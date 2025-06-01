using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge
{
    /// <summary>
    /// Cell for dictionaries, which will create a subtable where each key-value pair is a row.
    /// </summary>
    [CellType(TypeMatchMode.Assignable,typeof(IDictionary))]
    internal class DictionaryCell : CollectionCell
    {
        public DictionaryCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            CreateSubTable();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            CreateSubTable();
        }

        protected sealed override void CreateSubTable()
        {
            List<ITFSerializedObject> rowsData = new List<ITFSerializedObject>();

            if (Value == null || ((IDictionary)Value).Count == 0)
            {
                SubTable = TableGenerator.GenerateTable(new DictionaryColumnGenerator(), $"{Column.Table.Name}.{Column.Name}", this);
                return;
            }

            foreach (var key in ((IDictionary)Value).Keys)
            {
                rowsData.Add(new TFSerializedDictionaryItem((IDictionary)Value, key, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid));
            }
            
            if(SubTable != null)
                TableGenerator.GenerateTable(SubTable, rowsData);
            else 
                SubTable = TableGenerator.GenerateTable(rowsData, $"{Column.Table.Name}.{Column.Name}", this);
        }

        public override void AddItem(object key)
        {
            if(Value == null ||key == null || !Type.GenericTypeArguments[0].IsAssignableFrom(key.GetType()))
                return;
            
            if(((IDictionary)Value).Contains(key))
                return;
            
            ((IDictionary)Value).Add(key, null);
            TFSerializedDictionaryItem item = new TFSerializedDictionaryItem((IDictionary)Value, key, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid);
            TableGenerator.GenerateRow(SubTable, item);
        }

        public override void AddEmptyItem()
        {
            throw new NotSupportedException("Cannot add items to a dictionary without a key. Use AddItem(object key) instead.");
        }

        public override void RemoveItem(int position)
        {
            if(Value == null || position < 1 || position > SubTable.Rows.Count)
                return;
            
            var key = SubTable.Rows[position].Cells[1].GetValue();
            ((IDictionary)Value).Remove(key);
        }
        
        public override ICollection GetItems()
        {
            return Value.CreateShallowCopy() as ICollection;
        }

        protected override string SerializeCollection()
        {
            return SerializeDictionary(this.GetKeys(), this.GetValues());
        }
        
        private string SerializeDictionary(List<Cell> keys, List<Cell> values)
        {
            StringBuilder serializedData = new StringBuilder();
            serializedData.Append(SerializationConstants.JsonObjectStart);

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i].Serialize();
                string value = values[i]?.Serialize() ?? "null";
                
                if(values[i] is StringCell or EnumCell or LayerMaskCell) value = value.Replace('\'', '"'); 
                if(keys[i] is StringCell or EnumCell or LayerMaskCell) key = key.Trim('\'');

                serializedData.Append($"\"{key}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}");
            }

            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1);
            }

            serializedData.Append(SerializationConstants.JsonObjectEnd); 
            return serializedData.ToString();
        }
        
        public override void Deserialize(string data)
        {
        }

        protected override void DeserializeModifying(string[] values, ref int index)
        {
            DeserializeWithoutModifying(values, ref index);
        }
    }
}