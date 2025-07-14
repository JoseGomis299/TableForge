using System;
using System.Collections.Generic;

namespace TableForge.Editor.Serialization
{
    internal abstract class TableDeserializationArgs
    {
        private List<List<string>> _columnData;
        private List<string> _columnNames;
        private int _rowCount;
        
        public string Data { get; }
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
                    ExtractColumnData();
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
                    ExtractColumnNames();
                }
                return _columnNames;
            }
        }
        
        protected TableDeserializationArgs(string data, string tableName, string newElementsBasePath, string newElementsBaseName, SerializationFormat format, Type itemsType)
        {
            Data = data;
            TableName = tableName;
            NewElementsBasePath = newElementsBasePath;
            Format = format;
            ItemsType = itemsType;
            NewElementsBaseName = newElementsBaseName;
        }
        
        private void ExtractColumnData()
        {
            if (string.IsNullOrEmpty(Data))
                return;

            var extractor = ColumnExtractorFactory.CreateExtractor(this);
            _columnData = extractor?.ExtractColumnData(this);
        }

        private void ExtractColumnNames()
        {
            if (string.IsNullOrEmpty(Data))
                return;

            var extractor = ColumnExtractorFactory.CreateExtractor(this);
            _columnNames = extractor?.ExtractColumnNames(this);
        }
    }
    
    internal class JsonTableDeserializationArgs : TableDeserializationArgs
    {
        public JsonTableDeserializationArgs(string data, string tableName, string newElementsBasePath, string newElementsBaseName, Type itemsType) 
            : base(data, tableName, newElementsBasePath, newElementsBaseName,SerializationFormat.Json, itemsType) { }
    }
    
    internal class CsvTableDeserializationArgs : TableDeserializationArgs
    {
        public bool HasHeader { get; } 
        
        public CsvTableDeserializationArgs(string data, string tableName, string newElementsBasePath, string newElementsBaseName, Type itemsType, bool hasHeader) 
            : base(data, tableName, newElementsBasePath,newElementsBaseName, SerializationFormat.Csv, itemsType)
        {
            HasHeader = hasHeader;
        }
    }
}