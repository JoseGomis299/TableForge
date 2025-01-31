using System.Collections;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Cell for dictionaries, which will create a subtable where each key-value pair is a row.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(IDictionary))]
    [CellType(TypeMatchMode.GenericArgument,typeof(IDictionary<,>))]
    internal class DictionaryCell : SubTableCell
    {
        public DictionaryCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject)
        {
            CreateSubTable();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            CreateSubTable();
        }

        public override void SerializeData()
        {
        
        }

        protected sealed override void CreateSubTable()
        {
            List<ITFSerializedObject> rowsData = new List<ITFSerializedObject>();

            if(Value == null || ((IDictionary)Value).Count == 0) return;

            foreach (var key in ((IDictionary)Value).Keys)
            {
                rowsData.Add(new TFSerializedDictionaryItem((IDictionary)Value, key));
            }
            
            SubTable = TableGenerator.GenerateTable(rowsData, $"{Column.Table.Name}.{Column.Name}", this);
        }
    }
}