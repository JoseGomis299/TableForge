using System;
using System.Collections;
using System.Collections.Generic;
using TableForge.Exceptions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Serialized object for dictionary items, which have a key and a value
    /// </summary>
    internal class TFSerializedDictionaryItem : TFSerializedObject, ITFSerializedCollectionItem
    {
        private readonly IDictionary _dictionary;
        private object _itemKey;
        
        public TFSerializedDictionaryItem(IDictionary dictionary, object itemKey, Object rootObject) : base(dictionary, null, rootObject)
        {
            TargetInstance = dictionary;
            Name = "";
            _dictionary = dictionary;
            _itemKey = itemKey;
            ColumnGenerator = new DictionaryColumnGenerator();
        }
        
        public override object GetValue(Cell cell)
        {
            //This is a special case where the collection has been modified outside TableForge
            if(!_dictionary.Contains(_itemKey) && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: Adter this, the subtable will be regenerated
            }
            
            bool isKey = cell.Column.Name == "Key";
            return isKey ? _itemKey : _dictionary[_itemKey];
        }

        public override void SetValue(Cell cell, object data)
        {
            //This is a special case where the collection has been modified outside TableForge
            if(!_dictionary.Contains(_itemKey) && !((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid)
            {
                ((SubTableCell)cell.Row.Table.ParentCell).IsSubTableInvalid = true;
                //TODO: After this, the subtable will be regenerated
            }
            
            bool isKey = cell.Column.Name == "Key";
            if (isKey)
            {
                if(_dictionary.Contains(data))
                    throw new InvalidCellValueException($"Key {data} already exists in dictionary. Cannot add duplicate keys.");
                
                object value = _dictionary[_itemKey];
                _dictionary.Remove(_itemKey);
                _dictionary.Add(data, value);
                _itemKey = data;
            }
            else
            {
                _dictionary[_itemKey] = data;
            }

            if (!EditorUtility.IsDirty(RootObject))
                EditorUtility.SetDirty(RootObject);
        }

        public override Type GetValueType(Cell cell)
        {
            bool isKey = cell.Column.Name == "Key";
            if (isKey)
            {
                return _dictionary.GetType().GetGenericArguments()[0];
            }
            return _dictionary.GetType().GetGenericArguments()[1];
        }
        
        public override void PopulateRow(List<Column> columns, Table table, Row row)
        {
            ColumnGenerator.GenerateColumns(columns, table);
            row.SerializedObject = this;
            
            Cell keyCell = CellFactory.CreateCell(columns[0], row, _dictionary.GetType().GetGenericArguments()[0]);
            row.Cells.Add(1, keyCell);
            
            Cell valueCell = CellFactory.CreateCell(columns[1], row, _dictionary.GetType().GetGenericArguments()[1]);
            row.Cells.Add(2, valueCell);
        }
    }
}