using System.Collections.Generic;
using System.Linq;

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
        public IReadOnlyList<Cell> OrderedCells => Cells.OrderBy(x => x.Key).Select(x => x.Value).ToList();
        
        /// <summary>
        /// Collection of cells in the row, indexed by their column position.
        /// </summary>
        public Dictionary<int, Cell> Cells { get; } = new();
        
        /// <summary>
        /// The serialized object associated with the row.
        /// </summary>
        public ITFSerializedObject SerializedObject { get; }
        
        public Row(string name, int position, Table table, ITFSerializedObject serializedObject) : base(name, position, table)
        {
            SerializedObject = serializedObject;
            CalculateId();
            table.AddRow(this);
        }
        
        public void SetName(string name)
        {
            Name = name;
        }
        
        public void CalculateId()
        {
            string guid = SerializedObject.RootObjectGuid;
            
            if (!Table.IsSubTable)
            {
                Id = $"r:{guid}, t:{Table.Name}";
            }
            else
            {
                Id = $"r:{guid}, p:{Position}, t:{Table.Name}";
            }
        }
    }
}