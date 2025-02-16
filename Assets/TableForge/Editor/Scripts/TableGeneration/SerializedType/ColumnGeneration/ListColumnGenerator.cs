using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Generates columns for a list of values. Provides a single column for the list values.
    /// </summary>
    internal class ListColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<CellAnchor> columns, Table table)
        {
            if (columns.Count == 0)
            {
                columns.Add(new CellAnchor("Values", 1));
                table.AddColumn(columns[0]);
            }
        }
    }
}