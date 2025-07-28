using System;
using System.Collections;
using System.Collections.Generic;
using TableForge.Editor.Serialization;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for handling lists where the data is stored in a subtable in which each row represents an element in the list.
    /// </summary>
    [CellType(TypeMatchMode.Assignable, typeof(IList))]
    [CellType(TypeMatchMode.GenericArgument,typeof(IList<>))]
    internal class ListCell : CollectionCell
    {
        public ListCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new ListCellSerializer(this);
            CreateSubTable();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            CreateSubTable();
        }

        protected sealed override void CreateSubTable()
        {
            List<ITfSerializedObject> rowsData = new List<ITfSerializedObject>();
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];

            if (cachedValue == null || ((IList)cachedValue).Count == 0)
            {
                IColumnGenerator columnGenerator;
                if (itemType.IsSimpleType() || itemType.IsListOrArrayType())
                {
                    columnGenerator = new ListColumnGenerator();
                }
                else
                {
                    columnGenerator = new TfSerializedType(itemType, null);
                }

                SubTable = TableGenerator.GenerateTable(columnGenerator, $"{column.Table.Name}.{column.Name}", this);
                return;
            }
            
            for (var i = 0; i < ((IList)cachedValue).Count; i++)
            {
                rowsData.Add(new TfSerializedListItem((IList)cachedValue, ((IList)cachedValue)[i], i, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid));
            }
            
            // if(SubTable != null)
            //     TableGenerator.GenerateTable(SubTable, rowsData);
            // else 
                SubTable = TableGenerator.GenerateTable(rowsData, $"{column.Table.Name}.{column.Name}", this);
        }

        public override void AddItem(object item)
        {
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];
            if(!itemType.IsAssignableFrom(item.GetType()))
                throw new ArgumentException($"Item type {item.GetType()} is not assignable to list type {itemType}");
            
            if(cachedValue is Array array)
            {
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
                for (int i = 0; i < array.Length; i++)
                {
                    newArray.SetValue(array.GetValue(i), i);
                }
                
                array.SetValue(item, array.Length - 1);
                SetValue(newArray);
            }
            else if (cachedValue is IList list)
            {
                list.Add(item);
                TfSerializedListItem listItem = new TfSerializedListItem(list, item, list.Count - 1, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid);
                TableGenerator.GenerateRow(SubTable, listItem);
            }
        }

        public override void AddEmptyItem()
        {
            Type itemType = Type.IsArray ? Type.GetElementType() : Type.GetGenericArguments()[0];
            object item = ((IList)cachedValue).Count == 0 ? itemType.CreateInstanceWithDefaults() : ((IList)cachedValue)[^1].CreateShallowCopy();
            
            if(cachedValue is Array array)
            {
                Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
                for (int i = 0; i < array.Length; i++)
                {
                    newArray.SetValue(array.GetValue(i), i);
                }
                
                newArray.SetValue(item, array.Length);
                SetValue(newArray);
            }
            else if (cachedValue is IList list)
            {
                list.Add(item);
                TfSerializedListItem listItem = new TfSerializedListItem(((IList)cachedValue), ((IList)cachedValue)[^1], ((IList)cachedValue).Count - 1, TfSerializedObject.RootObject, TfSerializedObject.RootObjectGuid);
                TableGenerator.GenerateRow(SubTable, listItem);
            }
        }

        public override void RemoveItem(int position)
        {
            if(position < 1 || position > ((IList)cachedValue).Count)
                throw new IndexOutOfRangeException($"Index {position} is out of range for list of length {((IList)cachedValue).Count}");
            
            if (cachedValue is Array array)
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
            else if (cachedValue is IList list)
            {
                //Assuming that the rows are in the same order as the list
                for (int i = position; i < list.Count ; i++)
                {
                    ((TfSerializedListItem) SubTable.Rows[i].Cells[1].TfSerializedObject).CollectionIndex -= 1;
                }
                
                list.RemoveAt(position - 1);
            }
        }

        public override ICollection GetItems()
        {
            return cachedValue.CreateShallowCopy() as ICollection;
        }
    }
}