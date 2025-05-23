using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Represents a spreadsheet-style table with rows, columns, and cells, providing functionality for manipulating table structure and content.
    /// </summary>
    internal class Table
    {
        #region Fields

        private readonly Dictionary<int, Row> _rows = new Dictionary<int, Row>();
        private readonly Dictionary<int, Column> _columns = new Dictionary<int, Column>();

        private bool _rowsDirty = true;
        private bool _columnsDirty = true;
        private List<Row> _orderedRows = new List<Row>();
        private List<Column> _orderedColumns = new List<Column>();
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets read-only access to the table's rows by their numeric position.
        /// </summary>
        public IReadOnlyDictionary<int, Row> Rows => _rows;

        /// <summary>
        /// Gets read-only access to the table's columns by their numeric position.
        /// </summary>
        public IReadOnlyDictionary<int, Column> Columns => _columns;
        
        /// <summary>
        /// Gets the rows of the table in order of their position.
        /// </summary>
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

        /// <summary>
        /// Gets the columns of the table in order of their position.
        /// </summary>
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
        
        /// <summary>
        /// Gets the parent cell of the table, if any.
        /// <remarks>
        /// A table may be contained within a cell, this happens when a table represents a collection or a serialized object.
        /// </remarks>
        /// </summary>
        public Cell ParentCell { get; }
        
        /// <summary>
        /// Indicates whether the table is a sub-table contained within a cell.
        /// </summary>
        public bool IsSubTable => ParentCell != null;

        #endregion
        
        #region Constructors
        
        public Table(string name, Cell parentCell)
        {
            Name = parentCell != null ? $"{name}({parentCell.GetPosition()})" : name;
            ParentCell = parentCell;
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        ///  Adds a row to the table at the specified position, adjusting subsequent row positions accordingly.
        /// </summary>
        /// <param name="row">The row to add to the table.</param>
        /// <exception cref="ArgumentException">Throws for already existing rows.</exception>
        public void AddRow(Row row)
        {
            if (!_rows.TryAdd(row.Position, row))
                throw new ArgumentException("Row already exists in table");
            
            _rowsDirty = true;
        }
        
        /// <summary>
        ///  Adds a column to the table at the specified position, adjusting subsequent column positions accordingly.
        /// </summary>
        /// <param name="column">The column to add to the table.</param>
        /// <exception cref="ArgumentException">Throws for already existing columns.</exception>
        public void AddColumn(Column column)
        {
            if (!_columns.TryAdd(column.Position, column))
                throw new ArgumentException("Column already exists in table");
            
            _columnsDirty = true;
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
                bool isLastRemainingRow = _orderedRows.Count == 1;
                collectionCell.RemoveItem(position);
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
                
                _rows.Remove(_rows.Count);
            }
            
            _rowsDirty = true;
        }
        
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
        /// Moves a row from one position to another, adjusting subsequent row positions accordingly.
        /// </summary>
        /// <param name="fromPosition">Original 1-based row position</param>
        /// <param name="toPosition">New 1-based row position</param>
        /// <exception cref="ArgumentException">Thrown for invalid row positions</exception>
        public void MoveRow(int fromPosition, int toPosition)
        {
            MoveAnchor(fromPosition, toPosition, _rows, true);
            _rowsDirty = true;
        }

        /// <summary>
        /// Moves a column from one position to another, adjusting subsequent column positions accordingly.
        /// </summary>
        /// <param name="fromPosition">Original 1-based column position</param>
        /// <param name="toPosition">New 1-based column position</param>
        /// <exception cref="ArgumentException">Thrown for invalid column positions</exception>
        public void MoveColumn(int fromPosition, int toPosition)
        {
            MoveAnchor(fromPosition, toPosition, _columns, false);
            _columnsDirty = true;
        }

        /// <summary>
        /// Retrieves a cell from the table using spreadsheet-style position notation.
        /// </summary>
        /// <param name="position">Cell position in A1 notation (e.g., "B3")</param>
        /// <returns>Requested cell or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown for invalid position format</exception>
        public Cell GetCell(string position)
        {
            var (columnKey, rowKey) = PositionUtil.GetPosition(position);
            return Rows.TryGetValue(rowKey, out Row row) 
                ? row.Cells.GetValueOrDefault(columnKey) 
                : null;
        }
        
        /// <summary>
        /// Retrieves a cell from the table using numeric column and row positions.
        /// </summary>
        public Cell GetCell(int columnPos, int rowPos)
        {
            return Rows.TryGetValue(rowPos, out Row rowObj) 
                ? rowObj.Cells.GetValueOrDefault(columnPos) 
                : null;
        }
        

        #endregion

        #region Private Methods

        private void SwapRows(int fromPosition, int toPosition)
        {
            //Swap the values of the list if the rows represent list elements
            if (_columns.Count > 0
                 && _rows.TryGetValue(fromPosition, out Row fromRow)
                 && !fromRow.IsStatic
                 && fromRow.Cells.TryGetValue(1, out Cell fromCell)
                 && fromCell.TfSerializedObject is ITFSwapableCollectionItem fromItem
                 && _rows.TryGetValue(toPosition, out Row toRow)
                 && !toRow.IsStatic
                 && toRow.Cells.TryGetValue(1, out Cell toCell)
                 && toCell.TfSerializedObject is ITFSwapableCollectionItem toItem)
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