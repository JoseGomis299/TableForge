using System.Collections.Generic;

namespace TableForge.Editor
{
    internal class CsvColumnExtractor : ColumnExtractor
    {
        public override List<string> ExtractColumnNames(TableDeserializationArgs baseArgs)
        {
            var args = (CsvTableDeserializationArgs)baseArgs;
            var columnNames = new List<string>();
            if (string.IsNullOrEmpty(args.Data)) return columnNames;

            var rows = CsvParser.ParseCsv(args.Data);
            if (rows == null || rows.Count == 0) return columnNames;

            if (args.HasHeader)
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

        public override List<List<string>> ExtractColumnData(TableDeserializationArgs baseArgs)
        {
            var args = (CsvTableDeserializationArgs)baseArgs;
            var allRows = CsvParser.ParseCsv(args.Data);
            var columnData = new List<List<string>>();
            if (allRows == null || allRows.Count == 0) return columnData;

            int startRow = args.HasHeader ? 1 : 0;
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