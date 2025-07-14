using System.Collections.Generic;

namespace TableForge.Editor
{
    /// <summary>
    /// Generates columns for a list of values. Provides a single column for the list values.
    /// </summary>
    internal class ListColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<Column> columns, Table table)
        {
            if (columns.Count == 0)
            {
                columns.Add(new Column("Values", 1, table));
            }
        }
    }
}