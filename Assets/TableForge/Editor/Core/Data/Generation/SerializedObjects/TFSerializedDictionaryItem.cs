using System;
using System.Collections;
using System.Collections.Generic;
using TableForge.Editor.Exceptions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    /// <summary>
    /// Serialized object for dictionary items, which have a key and a value
    /// </summary>
    internal class TfSerializedDictionaryItem : TfSerializedObject, ITfSerializedCollectionItem
    {
        private readonly IDictionary _dictionary;
        private object _itemKey;
        
        public TfSerializedDictionaryItem(IDictionary dictionary, object itemKey, Object rootObject, string guid) : base(dictionary, null, rootObject, guid)
        {
            TargetInstance = dictionary;
            Name = itemKey.GetType().IsPrimitive ? itemKey.ToString() : itemKey.GetHashCode().ToString();
            _dictionary = dictionary;
            _itemKey = itemKey;
            columnGenerator = new DictionaryColumnGenerator();
        }
        
        public override object GetValue(Cell cell)
        {
            bool isKey = cell.column.Name == "Key";
            return isKey ? _itemKey : _dictionary[_itemKey];
        }

        public override void SetValue(Cell cell, object data)
        {
            bool isKey = cell.column.Name == "Key";
            if (isKey)
            {
                if(_dictionary.Contains(data) && (cell.GetValue() == null || !cell.GetValue().ToString().Equals(data.ToString())))
                    throw new InvalidCellValueException($"Key {data} already exists in dictionary. Cannot add duplicate keys.");
                
                object value = _dictionary[_itemKey];
                _dictionary.Remove(_itemKey);
                _dictionary.Add(data, value);
                _itemKey = data;
                
                cell.row.SetName(data.ToString());
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
            bool isKey = cell.column.Name == "Key";
            if (isKey)
            {
                return _dictionary.GetType().GetGenericArguments()[0];
            }
            return _dictionary.GetType().GetGenericArguments()[1];
        }
        
        public override void PopulateRow(List<Column> columns, Table table, Row row)
        {
            columnGenerator.GenerateColumns(columns, table);
            
            Cell keyCell = CellFactory.CreateCell(columns[0], row, _dictionary.GetType().GetGenericArguments()[0]);
            row.AddCell(1, keyCell);
            
            Cell valueCell = CellFactory.CreateCell(columns[1], row, _dictionary.GetType().GetGenericArguments()[1]);
            row.AddCell(2, valueCell);
        }
    }
}