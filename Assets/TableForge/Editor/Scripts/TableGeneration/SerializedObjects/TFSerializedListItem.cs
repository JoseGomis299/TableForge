using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Represents a list item in a serialized list.
    /// </summary>
    internal class TFSerializedListItem : TFSerializedObject, ITFSwapableCollectionItem, ITFSwapableCollectionItem<TFSerializedListItem>
    {
        private readonly bool _isSimpleValue;
        
        private readonly IList _collection;
        public int CollectionIndex { get; set; }
        
        public TFSerializedListItem(IList collection, object itemFromCollection, int collectionIndex, Object rootObject) : base(itemFromCollection, null, rootObject)
        {
            TargetInstance = collection;
            Name = "Element " + collectionIndex;
            _collection = collection;
            CollectionIndex = collectionIndex;
            Type itemType = collection.GetType().IsArray ?
                collection.GetType().GetElementType() 
                : collection.GetType().GetGenericArguments().FirstOrDefault();
            
            if (itemType.IsSimpleType() || typeof(Object).IsAssignableFrom(itemType) || itemType.IsListOrArrayType())
            {
                _isSimpleValue = true;
                ColumnGenerator = new ListColumnGenerator();
            }
            else 
            {
                TargetInstance = itemFromCollection;
                SerializedType = new TFSerializedType(itemType, null);
                ColumnGenerator = SerializedType;
            }
        }
        
        public override object GetValue(Cell cell)
        {
            //This is a special case where the collection has been modified outside TableForge
            if (_collection.Count < CollectionIndex && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: Adter this, the subtable will be regenerated
            }
            
            if (_isSimpleValue)
            {
                return _collection[CollectionIndex];
            }
            return base.GetValue(cell);
        }

        public override void SetValue(Cell cell, object data)
        {
            //This is a special case where the collection has been modified outside TableForge
            if (_collection.Count < CollectionIndex && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: Adter this, the subtable will be regenerated
            }
            
            if (_isSimpleValue)
            {
                _collection[CollectionIndex] = data;
                return;
            }
            
            base.SetValue(cell, data);

            if (SerializedType.IsStruct)
                _collection[CollectionIndex] = TargetInstance;
            
            if (!EditorUtility.IsDirty(RootObject))
                EditorUtility.SetDirty(RootObject);
        }

        public override Type GetValueType(Cell cell)
        {
            if(_collection.Count == 0)
            {
                return _collection.GetType().IsArray ?
                    _collection.GetType().GetElementType() 
                    : _collection.GetType().GetGenericArguments().FirstOrDefault();
            }
            
            if (_isSimpleValue)
            {
                return _collection[CollectionIndex].GetType();
            }
            return base.GetValueType(cell);
        }
        
        public override void PopulateRow(List<Column> columns, Table table, Row row)
        {
            if (!_isSimpleValue)
            {
                base.PopulateRow(columns, table, row);
                return;
            }
           
            ColumnGenerator.GenerateColumns(columns, table);

            Type memberType = _collection.GetType().IsArray ?
                _collection.GetType().GetElementType() 
                : _collection.GetType().GetGenericArguments().FirstOrDefault();

            Cell cell = CellFactory.CreateCell(columns[0], row, memberType);
            row.Cells.Add(1, cell);
        }

        public void SwapWith(ITFSwapableCollectionItem other)
        {
            SwapWith((TFSerializedListItem) other);
        }

        public void SwapWith(TFSerializedListItem other)
        {
            if (other == null)
            {
                return;
            }
            
            int index1 = CollectionIndex;
            int index2 = other.CollectionIndex;

            if (index1 < 0 || index1 >= _collection.Count || index2 < 0 || index2 >= _collection.Count)
            {
                return;
            }

            (_collection[index1], _collection[index2]) = (_collection[index2], _collection[index1]);
            
            if(!_isSimpleValue)
                (TargetInstance, other.TargetInstance) = (other.TargetInstance, TargetInstance);
            
            
            if (!EditorUtility.IsDirty(RootObject))
                EditorUtility.SetDirty(RootObject);
        }
    }
}