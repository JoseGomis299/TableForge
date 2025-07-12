using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor
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
        private readonly Dictionary<int, Cell> _cells;

        public override string Name
        {
            get
            {
                if(SerializedObject != null && SerializedObject is not ITfSerializedCollectionItem && SerializedObject.RootObject !=null)
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
                    _orderedCells = _cells.Values.OrderBy(cell => cell.column.Position).ToList();
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
        public ITfSerializedObject SerializedObject { get; }
        
        public Row(string name, int position, Table table, ITfSerializedObject serializedObject) : base(name, position, table)
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
                Id = HashCodeUtil.CombineHashes(guid, Position, Table.Name);
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
            if (!_cells.Remove(column, out Cell cell)) return;
            
            cell.UnregisterCell();
            _isDirty = true;
        }
        
        public void ClearCells()
        {
            foreach (var cell in _cells.Values)
            {
                cell.UnregisterCell();
            }
            _cells.Clear();
            _orderedCells.Clear();
            _isDirty = true;
        }
    }
}