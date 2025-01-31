using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Represents a list item in a serialized list.
    /// </summary>
    internal class TFSerializedListItem : TFSerializedObject
    {
        private readonly bool _isSimpleValue;
        
        private readonly IList _collection;
        private int _collectionIndex;
        
        public TFSerializedListItem(IList collection, object itemFromCollection, int collectionIndex)
        {
            TargetInstance = collection;
            Name = "Element " + collectionIndex;
            _collection = collection;
            _collectionIndex = collectionIndex;
            Type itemType = collection.GetType().IsArray ?
                collection.GetType().GetElementType() 
                : collection.GetType().GetGenericArguments().FirstOrDefault();
            
            if (itemType.IsSimpleType() || typeof(Object).IsAssignableFrom(itemType) || itemType.IsListOrArrayType())
            {
                _isSimpleValue = true;
            }
            else 
            {
                TargetInstance = itemFromCollection;
                Fields = SerializationUtil.GetSerializableFields(itemType);
            }
        }
        
        public override object GetValue(Cell cell)
        {
            //This is a special case where the collection has been modified outside TableForge
            if (_collection.Count < _collectionIndex && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: Adter this, the subtable will be regenerated
            }
            
            if (_isSimpleValue)
            {
                return _collection[_collectionIndex];
            }
            return base.GetValue(cell);
        }

        public override void SetValue(Cell cell, object data)
        {
            //This is a special case where the collection has been modified outside TableForge
            if (_collection.Count < _collectionIndex && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: Adter this, the subtable will be regenerated
            }
            
            if (_isSimpleValue)
            {
                _collection[_collectionIndex] = data;
                return;
            }
            base.SetValue(cell, data);
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
                return _collection[_collectionIndex].GetType();
            }
            return base.GetValueType(cell);
        }
        
        public override void PopulateRow(List<CellAnchor> columns, Table table, Row row)
        {
            if (!_isSimpleValue)
            {
                base.PopulateRow(columns, table, row);
                return;
            }
           
            if (columns.Count == 0)
            {
                columns.Add(new CellAnchor("Values", 1));
                table.AddColumn(columns[0]);
            }
            
            Type memberType = _collection.GetType().IsArray ?
                _collection.GetType().GetElementType() 
                : _collection.GetType().GetGenericArguments().FirstOrDefault();

            Cell cell = CellFactory.CreateCell(columns[0], row, memberType,null, this);
            row.Cells.Add(_collectionIndex + 1, cell);
        }

        public void SwapWith(TFSerializedListItem other)
        {
            if (other == null)
            {
                return;
            }
            
            int index1 = _collectionIndex;
            int index2 = other._collectionIndex;

            if (index1 < 0 || index1 >= _collection.Count || index2 < 0 || index2 >= _collection.Count)
            {
                return;
            }

            (_collection[index1], _collection[index2]) = (_collection[index2], _collection[index1]);

            Name = "Element " + index1;
            _collectionIndex = index1;
            
            other.Name = "Element " + index2;
            other._collectionIndex = index2;
        }
    }
}