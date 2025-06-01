using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge
{
    /// <summary>
    /// Cell for handling lists where the data is stored in a subtable in which each row represents an element in the list.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(IList))]
    [CellType(TypeMatchMode.GenericArgument,typeof(IList<>))]
    internal class ListCell : CollectionCell
    {
        public ListCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
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
            base.RefreshData();
            CreateSubTable();
        }

        protected sealed override void CreateSubTable()
        {
            List<ITFSerializedObject> rowsData = new List<ITFSerializedObject>();
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];

            if (Value == null || ((IList)Value).Count == 0)
            {
                IColumnGenerator columnGenerator;
                if (itemType.IsSimpleType() || typeof(UnityEngine.Object).IsAssignableFrom(itemType) ||
                    itemType.IsListOrArrayType())
                {
                    columnGenerator = new ListColumnGenerator();
                }
                else
                {
                    columnGenerator = new TFSerializedType(itemType, null);
                }

                SubTable = TableGenerator.GenerateTable(columnGenerator, $"{Column.Table.Name}.{Column.Name}", this);
                return;
            }
            
            for (var i = 0; i < ((IList)Value).Count; i++)
            {
                rowsData.Add(new TFSerializedListItem((IList)Value, ((IList)Value)[i], i, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid));
            }
            
            if(SubTable != null)
                TableGenerator.GenerateTable(SubTable, rowsData);
            else 
                SubTable = TableGenerator.GenerateTable(rowsData, $"{Column.Table.Name}.{Column.Name}", this);
        }

        public override void AddItem(object item)
        {
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];
            if(!itemType.IsAssignableFrom(item.GetType()))
                throw new ArgumentException($"Item type {item.GetType()} is not assignable to list type {itemType}");
            
            if(Value is Array array)
            {
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
                for (int i = 0; i < array.Length; i++)
                {
                    newArray.SetValue(array.GetValue(i), i);
                }
                
                array.SetValue(item, array.Length - 1);
                SetValue(newArray);
            }
            else if (Value is IList list)
            {
                list.Add(item);
                TFSerializedListItem listItem = new TFSerializedListItem(list, item, list.Count - 1, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid);
                TableGenerator.GenerateRow(SubTable, listItem);
            }
        }

        public override void AddEmptyItem()
        {
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];
            object item = ((IList)Value).Count == 0 ? itemType.CreateInstanceWithDefaults() : ((IList)Value)[^1].CreateShallowCopy();
            
            if(Value is Array array)
            {
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
                for (int i = 0; i < array.Length; i++)
                {
                    newArray.SetValue(array.GetValue(i), i);
                }
                
                newArray.SetValue(item, array.Length);
                SetValue(newArray);
            }
            else if (Value is IList list)
            {
                list.Add(item);
                TFSerializedListItem listItem = new TFSerializedListItem(((IList)Value), ((IList)Value)[^1], ((IList)Value).Count - 1, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid);
                TableGenerator.GenerateRow(SubTable, listItem);
            }
        }

        public override void RemoveItem(int position)
        {
            if(position < 1 || position > ((IList)Value).Count)
                throw new IndexOutOfRangeException($"Index {position} is out of range for list of length {((IList)Value).Count}");
            
            if (Value is Array array)
            {
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length - 1);
                for (int i = 0, j = 0; i < array.Length; i++)
                {
                    if (i == position - 1) continue;
                    newArray.SetValue(array.GetValue(i), j);
                    j++;
                }
                
                SetValue(newArray);
            }
            else if (Value is IList list)
            {
                //Assuming that the rows are in the same order as the list
                for (int i = position; i < list.Count ; i++)
                {
                    ((TFSerializedListItem) SubTable.Rows[i].Cells[1].TfSerializedObject).CollectionIndex -= 1;
                }
                
                list.RemoveAt(position - 1);
            }
        }

        public override ICollection GetItems()
        {
            return Value.CreateShallowCopy() as ICollection;
        }

        protected override string SerializeCollection()
        {
            return SerializeList(this.GetImmediateDescendants());
        }
        
        private string SerializeList(IEnumerable<Cell> collection)
        {
            var cells = collection.ToList();
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonArrayStart);
            int currentRow = -1;
            bool isSimpleType = cells.FirstOrDefault()?.Row.SerializedObject.SerializedType.Type.IsSimpleType() ?? false;

            foreach (var item in cells)
            {
                if (currentRow != item.Row.Position)
                {
                    if (currentRow != -1)
                    {
                        serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                        serializedData.Append(isSimpleType ? SerializationConstants.JsonItemSeparator : $"{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonItemSeparator}");
                    }

                    currentRow = item.Row.Position;

                    if (!isSimpleType && item.Row.OrderedCells.Count > 1)
                        serializedData.Append("{");
                }

                string value = isSimpleType
                    ? $"{item.Serialize()},"
                    : $"\"{item.Column.Name}\"{SerializationConstants.JsonKeyValueSeparator}{item.Serialize()}{SerializationConstants.JsonItemSeparator}";
                if(item is StringCell or EnumCell or LayerMaskCell) value = value.Replace('\'', '"'); 
                serializedData.Append(value);
            }

            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                if (!isSimpleType && cells.Last().Row.OrderedCells.Count > 1)
                    serializedData.Append(SerializationConstants.JsonObjectEnd);
            }

            serializedData.Append(SerializationConstants.JsonArrayEnd);
            return serializedData.ToString();
        }
    }
}