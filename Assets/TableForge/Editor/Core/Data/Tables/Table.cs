using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a spreadsheet-style table with rows, columns, and cells, providing functionality for manipulating table structure and content.
    /// </summary>
    internal class Table
    {
        #region Fields

        private readonly Dictionary<int, Row> _rows = new();
        private readonly Dictionary<string, Row> _rowsByName = new();
        private readonly Dictionary<int, Column> _columns = new();
        private readonly Dictionary<string, Column> _columnsByName = new();

        private bool _rowsDirty = true;
        private bool _columnsDirty = true;
        private List<Row> _orderedRows = new();
        private List<Column> _orderedColumns = new();
        
        #endregion

        #region Properties

        public string Name { get; }
        public IReadOnlyDictionary<int, Row> Rows => _rows;
        public IReadOnlyDictionary<int, Column> Columns => _columns;
        public IReadOnlyDictionary<string, Row> RowsByName => _rowsByName;
        public IReadOnlyDictionary<string, Column> ColumnsByName => _columnsByName;
        
        public IReadOnlyList<Row> OrderedRows
        {
            get
            {
                if (_rowsDirty)
                {
                    _orderedRows = new List<Row>(_rows.Values.OrderBy(x => x.Position));
                    _rowsDirty = false;
                }
                
                return _orderedRows;
            }
        }

        public IReadOnlyList<Column> OrderedColumns
        {
            get
            {
                if (_columnsDirty)
                {
                    _orderedColumns = new List<Column>(_columns.Values.OrderBy(x => x.Position));
                    _columnsDirty = false;
                }
                
                return _orderedColumns;
            }
        }
        
        public Cell ParentCell { get; }
        public bool IsSubTable => ParentCell != null;

        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the Table class.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        /// <param name="parentCell">The parent cell containing this table, if any.</param>
        public Table(string name, Cell parentCell)
        {
            Name = parentCell != null ? $"{name}({parentCell.column.Name} | {parentCell.row.Name})" : name;
            ParentCell = parentCell;
        }
        
        #endregion

        #region Public Methods - Row Management
        
        /// <summary>
        /// Adds a row to the table at the specified position, adjusting subsequent row positions accordingly.
        /// </summary>
        /// <param name="row">The row to add to the table.</param>
        /// <exception cref="ArgumentException">Thrown when a row already exists at the specified position.</exception>
        public void AddRow(Row row)
        {
            if (!_rows.TryAdd(row.Position, row))
                throw new ArgumentException("Row already exists in table");

            _rowsByName.TryAdd(row.Name, row);
            _rowsDirty = true;
        }
        
        /// <summary>
        /// Removes a row from the table at the specified position, adjusting subsequent row positions accordingly.
        /// <remarks>If the row represents a value in a collection, the corresponding value will be removed from it.</remarks>
        /// </summary>
        /// <param name="position">The position of the row to remove.</param>
        public void RemoveRow(int position)
        {
            if (!_rows.TryGetValue(position, out Row row))
                return;

            if (ParentCell is ICollectionCell collectionCell)
            {
                collectionCell.RemoveItem(position);
                _rows[position].ClearCells();
                _rows.Remove(position);
            }
            else
            {
                for (int i = position; i <= OrderedRows.Count; i++)
                {
                    OrderedRows[i - 1].Position -= 1;
                    if (i < OrderedRows.Count)
                    {
                        _rows[i] = _rows[i + 1];
                        _rows[i].Position = i;
                    }
                }
                
                _rows[_rows.Count].ClearCells();
                _rows.Remove(_rows.Count);
            }
            
            _rowsByName.Remove(row.Name);
            _rowsDirty = true;
        }
        
        /// <summary>
        /// Moves a row from one position to another, adjusting subsequent row positions accordingly.
        /// </summary>
        /// <param name="fromPosition">Original 1-based row position.</param>
        /// <param name="toPosition">New 1-based row position.</param>
        /// <exception cref="ArgumentException">Thrown for invalid row positions.</exception>
        public void MoveRow(int fromPosition, int toPosition)
        {
            MoveAnchor(fromPosition, toPosition, _rows, true);
            _rowsDirty = true;
        }

        #endregion

        #region Public Methods - Column Management
        
        /// <summary>
        /// Adds a column to the table at the specified position, adjusting subsequent column positions accordingly.
        /// </summary>
        /// <param name="column">The column to add to the table.</param>
        /// <exception cref="ArgumentException">Thrown when a column already exists at the specified position.</exception>
        public void AddColumn(Column column)
        {
            if (!_columns.TryAdd(column.Position, column))
                throw new ArgumentException("Column already exists in table");

            _columnsByName.TryAdd(column.Name, column);
            _columnsDirty = true;
        }

        /// <summary>
        /// Moves a column from one position to another, adjusting subsequent column positions accordingly.
        /// </summary>
        /// <param name="fromPosition">Original 1-based column position.</param>
        /// <param name="toPosition">New 1-based column position.</param>
        /// <exception cref="ArgumentException">Thrown for invalid column positions.</exception>
        public void MoveColumn(int fromPosition, int toPosition)
        {
            MoveAnchor(fromPosition, toPosition, _columns, false);
            _columnsDirty = true;
        }

        #endregion

        #region Public Methods - Cell Access
        
        /// <summary>
        /// Retrieves a cell from the table using spreadsheet-style position notation.
        /// For nested tables, the position can be in the format "A1.B2".
        /// </summary>
        /// <param name="position">Cell position in A1 notation (e.g., "B3" or "B3.A1" for nested cells).</param>
        /// <returns>Requested cell or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown for invalid position format.</exception>
        public Cell GetCell(string position)
        {
            if (string.IsNullOrEmpty(position))
                return null;

            Table table = this;
            while (position.Contains("."))
            {
                int dotIndex = position.IndexOf('.');
                string cellPos = position.Substring(0, dotIndex);
                position = position.Substring(dotIndex + 1);

                var (col, row) = PositionUtil.GetPosition(cellPos);
                Cell cell = table.GetCell(col, row);
                if (cell is SubTableCell subTableCell)
                    table = subTableCell.SubTable;
                else
                    return null;
            }

            var (colPos, rowPos) = PositionUtil.GetPosition(position);
            return table.GetCell(colPos, rowPos);
        }

        /// <summary>
        /// Retrieves a cell from the table using column and row positions.
        /// </summary>
        /// <param name="columnPos">1-based column position.</param>
        /// <param name="rowPos">1-based row position.</param>
        /// <returns>The cell at the specified position or null if not found.</returns>
        public Cell GetCell(int columnPos, int rowPos)
        {
            return Rows.TryGetValue(rowPos, out Row rowObj) 
                ? rowObj.Cells.GetValueOrDefault(columnPos) 
                : null;
        }

        #endregion

        #region Public Methods - Table Management
        
        /// <summary>
        /// Clears all rows and columns from the table without removing the referenced data.
        /// </summary>
        public void Clear()
        {
            _rows.Clear();
            _columns.Clear();
            _orderedRows.Clear();
            _orderedColumns.Clear();
            _rowsDirty = true;
            _columnsDirty = true;
        }
        
        /// <summary>
        /// Sets the order of rows based on the provided positions.
        /// </summary>
        /// <param name="positions">List of row positions in the desired order.</param>
        public void SetRowOrder(IList<int> positions)
        {
            if(positions.Count != _rows.Count)
                throw new ArgumentException("Invalid number of positions provided for rows.");
            
            Dictionary<int, Row> newRows = new Dictionary<int, Row>();
            HashSet<int> addedRows = new HashSet<int>(); 
            for (int i = 0; i < positions.Count; i++)
            {
                int position = positions[i];
                if (!_rows.TryGetValue(position, out Row row))
                    throw new ArgumentException($"Row with position {position} does not exist in the table.");
                if (!addedRows.Add(row.Id))
                    throw new ArgumentException($"Row with position {position} has already been added to the new order.");

                row.Position = i + 1; // Convert to 1-based index
                newRows.Add(i + 1, row);
            }

            _rows.Clear();
            foreach (var kvp in newRows)
            {
                _rows[kvp.Key] = kvp.Value;
            }
            _rowsDirty = true;
        }

        #endregion

        #region Private Methods - Anchor Management
        
        private void SwapRows(int fromPosition, int toPosition)
        {
            //Swap the values of the list if the rows represent list elements
            if (_columns.Count > 0
                 && _rows.TryGetValue(fromPosition, out Row fromRow)
                 && !fromRow.IsStatic
                 && fromRow.Cells.TryGetValue(1, out Cell fromCell)
                 && fromCell.TfSerializedObject is ITfSwapableCollectionItem fromItem
                 && _rows.TryGetValue(toPosition, out Row toRow)
                 && !toRow.IsStatic
                 && toRow.Cells.TryGetValue(1, out Cell toCell)
                 && toCell.TfSerializedObject is ITfSwapableCollectionItem toItem)
            {
                fromItem.SwapWith(toItem);
            }
            else //If not, just swap the rows
            {
                SwapAnchors(fromPosition, toPosition, _rows);
            }
        }

        private void SwapColumns(int fromPosition, int toPosition)
        {
            SwapAnchors(fromPosition, toPosition, _columns);
            
            foreach (Row row in _rows.Values)
            {
                Cell fromCell = row.Cells[fromPosition];
                Cell toCell = row.Cells[toPosition];
                
                row.AddCell(fromPosition, toCell);
                row.AddCell(toPosition, fromCell);
            }
        }

        private void SwapAnchors<T>(int fromPosition, int toPosition, Dictionary<int, T> anchors) where T : CellAnchor
        {
            T fromAnchor = anchors[fromPosition];
            T toAnchor = anchors[toPosition];
            
            anchors[fromPosition] = toAnchor;
            anchors[toPosition] = fromAnchor;
            
            fromAnchor.Position = toPosition;
            toAnchor.Position = fromPosition;
        }

        private void MoveAnchor<T>(int fromPosition, int toPosition, Dictionary<int, T> anchors, bool isRow) where T : CellAnchor
        {
            if(fromPosition == toPosition)
                return;
            
            if (!anchors.ContainsKey(fromPosition) || !anchors.ContainsKey(toPosition))
                throw new ArgumentException("Invalid position " + fromPosition + " or " + toPosition);
            
            T currentAnchor = anchors[fromPosition];
            if (currentAnchor.IsStatic)
            {
                Debug.LogWarning($"Cannot move static anchor with name {currentAnchor.Name} in table {Name}");
                return;
            }
            
            bool isMovingForward = fromPosition < toPosition;

            Action<int, int> swapMethod = isRow ? SwapRows : SwapColumns; 
            
            while (currentAnchor.Position != toPosition)
            {
                int nextPosition = isMovingForward ? currentAnchor.Position + 1 : currentAnchor.Position - 1;
                if(anchors.ContainsKey(nextPosition) && anchors[nextPosition].IsStatic)
                    continue;
                
                swapMethod.Invoke(currentAnchor.Position, nextPosition);
                currentAnchor = anchors[nextPosition];
            }
        }

        #endregion
    }
}