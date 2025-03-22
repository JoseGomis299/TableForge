using System;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Concrete implementation of <see cref="CellAnchor"/> representing a row in a table.
    /// <remarks>
    /// A row is a collection of cells, each of which may contain data.
    /// </remarks>
    /// </summary>
    internal class Row : CellAnchor
    {
        /// <summary>
        /// Collection of cells in the row, indexed by their column position.
        /// </summary>
        public Dictionary<int, Cell> Cells { get; } = new Dictionary<int, Cell>();
        
        /// <summary>
        /// The serialized object associated with the row.
        /// </summary>
        public ITFSerializedObject SerializedObject { get; set; }

        public Row(string name, int position, Table table) : base(name, position, table)
        { 
            Id = HashCodeUtil.CombineHashes(name, position, true, table.Name);
            table.AddRow(this);
        }
    }
}