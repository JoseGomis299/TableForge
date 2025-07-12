using System.Collections.Generic;

namespace TableForge.Editor
{
    /// <summary>
    /// Generates columns for dictionary sub tables, providing a key and a value column.
    /// </summary>
    internal class DictionaryColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<Column> columns, Table table)
        {
            if (columns.Count != 0) return;
            Column keyColumn = new Column("Key", 1, table);
            Column valueColumn = new Column("Value", 2, table);
            keyColumn.IsStatic = true;
            valueColumn.IsStatic = true;
                
            columns.Add(keyColumn);
            columns.Add(valueColumn);
        }
    }
}