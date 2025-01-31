using System;
using System.Collections.Generic;
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
        private readonly Dictionary<int, CellAnchor> _columns = new Dictionary<int, CellAnchor>();

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
        public IReadOnlyDictionary<int, CellAnchor> Columns => _columns;
        
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
            Name = name;
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

            row.Table = this;
        }
        
        /// <summary>
        ///  Adds a column to the table at the specified position, adjusting subsequent column positions accordingly.
        /// </summary>
        /// <param name="column">The column to add to the table.</param>
        /// <exception cref="ArgumentException">Throws for already existing columns.</exception>
        public void AddColumn(CellAnchor column)
        {
            if (!_columns.TryAdd(column.Position, column))
                throw new ArgumentException("Column already exists in table");

            column.Table = this;
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
        }

        /// <summary>
        /// Retrieves a cell from the table using spreadsheet-style position notation.
        /// </summary>
        /// <param name="position">Cell position in A1 notation (e.g., "B3")</param>
        /// <returns>Requested cell or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown for invalid position format</exception>
        public Cell GetCell(string position)
        {
            var (rowKey, columnKey) = PositionUtil.GetPosition(position);
            return Rows.TryGetValue(rowKey, out Row row) 
                ? row.Cells.GetValueOrDefault(columnKey) 
                : null;
        }

        /// <summary>
        /// Placeholder for serialization functionality. Currently not implemented.
        /// </summary>
        public void Serialize()
        {
            // TODO: Implement serialization logic
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
                 && fromCell.TfSerializedObject is TFSerializedListItem fromItem
                 && _rows.TryGetValue(toPosition, out Row toRow)
                 && !toRow.IsStatic
                 && toRow.Cells.TryGetValue(1, out Cell toCell)
                 && toCell.TfSerializedObject is TFSerializedListItem toItem)
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
                
                row.Cells[fromPosition] = toCell;
                row.Cells[toPosition] = fromCell;
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
                throw new ArgumentException("Invalid label position");
            
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