using System;
using System.Collections.Generic;

namespace TableForge.Editor.Serialization
{
    internal class CsvTableDeserializer : TableDeserializer
    {
        public bool HasHeader { get; } 
        
        public CsvTableDeserializer(string data, string tableName, string newElementsBasePath, string newElementsBaseName, Type itemsType, bool hasHeader) 
            : base(data, tableName, newElementsBasePath,newElementsBaseName, SerializationFormat.Csv, itemsType)
        {
            HasHeader = hasHeader;
        }
        
        protected override List<string> ExtractColumnNames()
        {
            var columnNames = new List<string>();
            if (string.IsNullOrEmpty(Data)) return columnNames;

            var rows = CsvParser.ParseCsv(Data);
            if (rows == null || rows.Count == 0) return columnNames;

            if (HasHeader)
            {
                columnNames = rows[0];
            }
            else
            {
                for (int i = 0; i < rows[0].Count; i++)
                    columnNames.Add((i + 1).ToString());
            }

            return columnNames;
        }

        protected override List<List<string>> ExtractColumnData()
        {
            var allRows = CsvParser.ParseCsv(Data);
            var columnData = new List<List<string>>();
            if (allRows == null || allRows.Count == 0) return columnData;

            int startRow = HasHeader ? 1 : 0;
            int columnCount = allRows[0].Count;

            for (int col = 0; col < columnCount; col++)
            {
                var column = new List<string>();
                for (int row = startRow; row < allRows.Count; row++)
                {
                    if (col < allRows[row].Count)
                        column.Add(allRows[row][col]);
                }
                columnData.Add(column);
            }

            return columnData;
        }
    }
}