using System.Collections;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Cell for dictionaries, which will create a subtable where each key-value pair is a row.
    /// </summary>
    [CellType(TypeMatchMode.Assignable,typeof(IDictionary))]
    internal class DictionaryCell : SubTableCell, ICollectionCell
    {
        public DictionaryCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            CreateSubTable();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            CreateSubTable();
        }

        public override void RefreshData()
        {
        
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
                rowsData.Add(new TFSerializedDictionaryItem((IDictionary)Value, key, TfSerializedObject.RootObject));
            }
            
            if(SubTable != null)
                TableGenerator.GenerateTable(SubTable, rowsData);
            else 
                SubTable = TableGenerator.GenerateTable(rowsData, $"{Column.Table.Name}.{Column.Name}", this);
        }

        public void AddItem(object key)
        {
            if(Value == null ||key == null || !Type.GenericTypeArguments[0].IsAssignableFrom(key.GetType()))
                return;
            
            if(((IDictionary)Value).Contains(key))
                return;
            
            ((IDictionary)Value).Add(key, null);
            TFSerializedDictionaryItem item = new TFSerializedDictionaryItem((IDictionary)Value, key, TfSerializedObject.RootObject);
            TableGenerator.GenerateRow(SubTable, item);
        }

        public void AddEmptyItem()
        {
            throw new System.NotSupportedException("Cannot add items to a dictionary without a key. Use AddItem(object key) instead.");
        }

        public void RemoveItem(int position)
        {
            if(Value == null || position < 1 || position > SubTable.Rows.Count)
                return;
            
            var key = SubTable.Rows[position].Cells[1].GetValue();
            ((IDictionary)Value).Remove(key);
        }
    }
}