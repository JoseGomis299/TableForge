using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Generates columns for dictionary sub tables, providing a key and a value column.
    /// </summary>
    internal class DictionaryColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<CellAnchor> columns, Table table)
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
        }
    }
}