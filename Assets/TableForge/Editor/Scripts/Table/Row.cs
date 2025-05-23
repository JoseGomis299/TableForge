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
        private bool _isDirty;
        private List<Cell> _orderedCells;
        private Dictionary<int, Cell> _cells;

        public override string Name
        {
            get
            {
                if(SerializedObject != null && SerializedObject is not ITFSerializedCollectionItem)
                    return SerializedObject.RootObject.name;
                
                return base.Name;
            }
            protected set => base.Name = value;
        }

        /// <summary>
        /// Collection of cells in the row, indexed by their column position.
        /// </summary>
        public IReadOnlyList<Cell> OrderedCells
        {
            get
            {
                if (_isDirty)
                {
                    _orderedCells = _cells.Values.OrderBy(cell => cell.Column.Position).ToList();
                    _isDirty = false;
                }
                return _orderedCells;
            }
        }
        
        /// <summary>
        /// Collection of cells in the row, indexed by their column position.
        /// </summary>
        public IReadOnlyDictionary<int, Cell> Cells => _cells;
        
        /// <summary>
        /// The serialized object associated with the row.
        /// </summary>
        public ITFSerializedObject SerializedObject { get; }
        
        public Row(string name, int position, Table table, ITFSerializedObject serializedObject) : base(name, position, table)
        {
            _cells = new Dictionary<int, Cell>();
            _orderedCells = new List<Cell>();
            
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
                Id = HashCodeUtil.CombineHashes(guid);
            }
            else
            {
                Id = HashCodeUtil.CombineHashes(guid, Position, true, Table.Name);
            }
        }
        
        public void AddCell(int column, Cell cell)
        {
            if (!_cells.TryAdd(column, cell))
            {
                _cells[column] = cell;
            }
            
            _isDirty = true;
        }
        
        public void RemoveCell(int column)
        {
            if (!_cells.Remove(column, out _)) return;
            _isDirty = true;
        }
    }
}