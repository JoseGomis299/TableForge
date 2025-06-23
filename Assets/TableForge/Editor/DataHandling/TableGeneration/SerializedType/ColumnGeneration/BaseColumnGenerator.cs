using System.Collections.Generic;
using TableForge.Editor;

namespace TableForge
{
    /// <summary>
    /// A base column generator that does not generate any columns.
    /// </summary>
    internal class BaseColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<Column> columns, Table table)
        {
            // Do nothing
        }
    }
}