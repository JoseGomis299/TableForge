using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor
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
        /// <param name="table">The table to populate.</param>
        /// <returns>A new <see cref="Table"/> populated with the serialized data.</returns>
        public static Table GenerateTable(Table table, List<ITfSerializedObject> items)
        {
            int rowCount = items.Count;
            List<Column> columns = new List<Column>();
            table.Clear();
            
            for (int i = 0; i < rowCount; i++)
            {
                Row row = new Row(items[i].Name, i + 1, table, items[i]);
                items[i].PopulateRow(columns, table, row);
            }
            
            return table;
        }
        
        
        /// <summary>
        /// Generates a table from a list of serialized objects.
        /// </summary>
        /// <param name="items">A list of serialized objects to populate the table.</param>
        /// <param name="tableName">The name of the generated table.</param>
        /// <param name="parentCell">The parent cell for the table, if any.</param>
        /// <returns>A new <see cref="Table"/> populated with the serialized data.</returns>
        public static Table GenerateTable(List<ITfSerializedObject> items, string tableName, Cell parentCell)
        {
            Table table = new Table(tableName, parentCell);
            return GenerateTable(table, items);
        }
        
        
        /// <summary>
        /// Generates an empty table with columns based on the provided column generator.
        /// </summary>
        /// <param name="columnGenerator">The provided generator to create the corresponding columns.</param>
        /// <param name="tableName">The name of the generated table.</param>
        /// <param name="parentCell">The parent cell for the table, if any.</param>
        /// <returns>A new <see cref="Table"/> without data but with the correct columns.</returns>
        public static Table GenerateTable(IColumnGenerator columnGenerator, string tableName, Cell parentCell)
        {
            if (columnGenerator == null)
                return null;

            List<Column> columns = new List<Column>();
            Table table = new Table(tableName, parentCell);
            columnGenerator.GenerateColumns(columns, table);
            return table;
        }

        /// <summary>
        /// Generates a row for a given table and serialized object.
        /// </summary>
        /// <param name="table">The table where the row will be added.</param>
        /// <param name="item">The serialized object used to populate the row.</param>
        /// <returns>A new <see cref="Row"/> populated with the serialized data.</returns>
        public static Row GenerateRow(Table table, ITfSerializedObject item)
        {
            if (item == null)
                return null;

            List<Column> columns = table.Columns.Values.ToList();
            Row row = new Row(item.Name, table.Rows.Count + 1, table, item);
            item.PopulateRow(columns, table, row);
            return row;
        }
    }
}
