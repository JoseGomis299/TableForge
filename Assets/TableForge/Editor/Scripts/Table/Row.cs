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
        
        public Row(string name, int position) : base(name, position) { }
    }
}