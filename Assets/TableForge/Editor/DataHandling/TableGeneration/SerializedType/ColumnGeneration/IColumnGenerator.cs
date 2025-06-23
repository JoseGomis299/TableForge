using System.Collections.Generic;

namespace TableForge.Editor
{
    /// <summary>
    /// Interface for generating columns for a table.
    /// </summary>
    internal interface IColumnGenerator
    {
        
        /// <summary>
        /// Generates columns for a table and adds them to the provided list.
        /// </summary>
        /// <param name="columns">The list to store the generated columns.</param>
        /// <param name="table">The table where the columns will be placed.</param>
        void GenerateColumns(List<Column> columns, Table table);
    }
}