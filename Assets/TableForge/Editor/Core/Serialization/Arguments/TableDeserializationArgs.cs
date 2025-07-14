using System;
using System.Collections.Generic;

namespace TableForge.Editor.Serialization
{
    internal abstract class TableDeserializationArgs
    {
        private List<List<string>> _columnData;
        private List<string> _columnNames;
        
        public string Data { get; }
        public string TableName { get; }
        public Type ItemsType { get; }
        public string NewElementsBasePath { get; }
        public string NewElementsBaseName { get; }
        public SerializationFormat Format { get; }
        
        
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