using System.Collections;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Cell for handling lists where the data is stored in a subtable in which each row represents an element in the list.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(IList))]
    [CellType(TypeMatchMode.GenericArgument,typeof(IList<>))]
    internal class ListCell : SubTableCell
    {
        public ListCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject)
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

            if(Value == null || ((IList)Value).Count == 0) return;
            
            for (var i = 0; i < ((IList)Value).Count; i++)
            {
                rowsData.Add(new TFSerializedListItem((IList)Value, ((IList)Value)[i], i));
            }
            
            SubTable = TableGenerator.GenerateTable(rowsData, $"{Column.Table.Name}.{Column.Name}", this);
        }
    }
}