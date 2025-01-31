using System;
using System.Collections;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Serialized object for dictionary items, which have a key and a value
    /// </summary>
    internal class TFSerializedDictionaryItem : TFSerializedObject
    {
        private readonly IDictionary _dictionary;
        private object _itemKey;
        
        public TFSerializedDictionaryItem(IDictionary dictionary, object itemKey)
        {
            TargetInstance = dictionary;
            Name = "";
            _dictionary = dictionary;
            _itemKey = itemKey;
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
                //TODO: Adter this, the subtable will be regenerated
            }
            
            bool isKey = cell.Column.Name == "Key";
            if (isKey)
            {
                object value = _dictionary[_itemKey];
                _dictionary.Remove(_itemKey);
                _dictionary.Add(data, value);
                _itemKey = data;
            }
            else
            {
                _dictionary[_itemKey] = data;
            }
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
        
        public override void PopulateRow(List<CellAnchor> columns, Table table, Row row)
        {
            CellAnchor keyColumn = new CellAnchor("Key", 1);
            CellAnchor valueColumn = new CellAnchor("Value", 2);
            keyColumn.IsStatic = true;
            valueColumn.IsStatic = true;
            
            if(columns.Count == 0)
            {
                columns.Add(keyColumn);
                columns.Add(valueColumn);
                
                table.AddColumn(keyColumn);
                table.AddColumn(valueColumn);
            }
            
            Cell keyCell = CellFactory.CreateCell(columns[0], row, _dictionary.GetType().GetGenericArguments()[0],null, this);
            row.Cells.Add(1, keyCell);
            
            Cell valueCell = CellFactory.CreateCell(columns[1], row, _dictionary.GetType().GetGenericArguments()[1],null, this);
            row.Cells.Add(2, valueCell);
        }
        
    }
}