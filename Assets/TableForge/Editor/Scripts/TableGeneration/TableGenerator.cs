using System.Collections.Generic;
using System.Linq;

namespace TableForge
{
    /// <summary>
    /// A static class responsible for generating tables and rows from serialized objects.
    /// </summary>
    internal static class TableGenerator
    {
        /// <summary>
        /// Generates a table from a list of serialized objects.
        /// </summary>
        /// <param name="items">A list of serialized objects to populate the table.</param>
        /// <param name="tableName">The name of the generated table.</param>
        /// <param name="parentCell">The parent cell for the table, if any.</param>
        /// <returns>A new <see cref="Table"/> populated with the serialized data.</returns>
        public static Table GenerateTable(List<ITFSerializedObject> items, string tableName, Cell parentCell)
        {
            if (items == null || items.Count == 0)
                return null; 
            
            int rowCount = items.Count;
            List<CellAnchor> columns = new List<CellAnchor>();
            Table table = new Table(tableName, parentCell);
            
            for (int i = 0; i < rowCount; i++)
            {
                Row row = new Row(items[i].Name, i + 1);
                table.AddRow(row);
                items[i].PopulateRow(columns, table, row);
            }
            
            return table;
        }

        /// <summary>
        /// Generates a row for a given table and serialized object.
        /// </summary>
        /// <param name="table">The table where the row will be added.</param>
        /// <param name="item">The serialized object used to populate the row.</param>
        /// <returns>A new <see cref="Row"/> populated with the serialized data.</returns>
        public static Row GenerateRow(Table table, ITFSerializedObject item)
        {
            if (item == null)
                return null;

            List<CellAnchor> columns = table.Columns.Values.ToList();
            Row row = new Row(item.Name, table.Rows.Count + 1);
            table.AddRow(row);
            item.PopulateRow(columns, table, row);
            return row;
        }
    }
}
