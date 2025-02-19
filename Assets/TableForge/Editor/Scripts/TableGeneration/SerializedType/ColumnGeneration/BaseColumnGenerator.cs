using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// A base column generator that does not generate any columns.
    /// </summary>
    internal class BaseColumnGenerator : IColumnGenerator
    {
        public void GenerateColumns(List<CellAnchor> columns, Table table)
        {
            Debug.Log("VAR");
            // Do nothing
        }
    }
}