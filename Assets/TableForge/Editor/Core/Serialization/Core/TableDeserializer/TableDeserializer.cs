using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.Serialization
{
    internal abstract class TableDeserializer
    {
        private List<List<string>> _columnData;
        private List<string> _columnNames;
        private int _rowCount;
        
        public string Data { get; protected set; }
        public string TableName { get; }
        public Type ItemsType { get; }
        public string NewElementsBasePath { get; }
        public string NewElementsBaseName { get; }
        public SerializationFormat Format { get; }
        
        public int RowCount 
        {
            get
            {
                if(_rowCount > 0)
                    return _rowCount;
                
                int count = 0;
                foreach (var column in ColumnData)
                {
                    if (column != null)
                        count = column.Count;
                    
                    if (count > 0)
                        break;
                }

                _rowCount = count;
                return _rowCount;
            }
        }
        
        public List<List<string>> ColumnData 
        {
            get
            {
                if (_columnData == null)
                {
                    _columnData = ExtractColumnData();
                }
                return _columnData;
            }
        }
        public List<string> ColumnNames 
        {
            get
            {
                if (_columnNames == null)
                {
                    _columnNames = ExtractColumnNames();
                }
                return _columnNames;
            }
        }
        
        protected TableDeserializer(string data, string tableName, string newElementsBasePath, string newElementsBaseName, SerializationFormat format, Type itemsType)
        {
            Data = data;
            TableName = tableName;
            NewElementsBasePath = newElementsBasePath;
            Format = format;
            ItemsType = itemsType;
            NewElementsBaseName = newElementsBaseName;
        }
        
        protected abstract List<string> ExtractColumnNames();
        protected abstract List<List<string>> ExtractColumnData();

        public void Deserialize(Table table, SerializationOptions options , int[] columnIndicesMapping = null)
        {
            if (table == null) return;
            
            List<List<string>> processedColumnData = new List<List<string>>();
            if(columnIndicesMapping == null || columnIndicesMapping.Length == 0)
            {
                processedColumnData = ColumnData;
            }
            else
            {
                //Reorder the data based on the provided column indices
                var columnDataCopy = ColumnData.ToList();
                for (var i = 0; i < columnIndicesMapping.Length; i++)
                {
                    int newIndex = columnIndicesMapping[i];
                    if (newIndex == -1) continue;
                
                    processedColumnData.Add(columnDataCopy[newIndex]);
                }
            }
            
            // Deserialize the data into the table
            for (int i = 0; i < table.OrderedRows.Count; i++)
            {
                Row row = table.OrderedRows[i];
                for (int j = 0; j < processedColumnData.Count; j++)
                {
                    if (processedColumnData[j] == null) continue;
                    
                    string cellValue = processedColumnData[j][i];
                    if (string.IsNullOrEmpty(cellValue)) continue;
                    
                    Cell cell = row.OrderedCells[j]; 
                    if(!cell.Serializer.TryDeserialize(cellValue, options))
                    {
                        Debug.LogWarning($"Failed to deserialize cell value '{cellValue}' for cell {cell.GetGlobalPosition()}.");
                    }
                }
            }
        }
    }
}