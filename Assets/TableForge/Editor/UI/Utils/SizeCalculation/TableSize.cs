using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class TableSize
    {
        private readonly Dictionary<int, Dictionary<int, Vector2>> _rowSizes = new();
        private readonly Dictionary<int, Dictionary<int, Vector2>> _columnSizes = new();
        private readonly Dictionary<int, Vector2> _rowPreferredSizes = new();
        private readonly Dictionary<int, Vector2> _columnPreferredSizes = new();
        
        private readonly TableMetadata _tableMetadata;
        private readonly TableAttributes _tableAttributes;
        private readonly Table _table;
        
        public TableSize(Table table, TableMetadata tableMetadata, TableAttributes tableAttributes)
        {
            _table = table;
            _tableMetadata = tableMetadata;
            _tableAttributes = tableAttributes;
        }
        
        public Vector2 GetTotalSize(bool useStoredValues, HashSet<int> headersToIgnore = null)
        {
            bool transposed = !_table.IsSubTable && _tableMetadata.IsTransposed;
            float width = 0, height = 0;
            
            foreach (var row in _rowPreferredSizes)
            {
                if (_tableAttributes.columnHeaderVisibility == TableHeaderVisibility.Hidden && row.Key == 0) 
                    continue;
                if (headersToIgnore != null && headersToIgnore.Contains(row.Key))
                    continue;

                int rowId = row.Key == 0 && _table.IsSubTable ? _table.ParentCell.Id : row.Key;
                if (transposed)
                {
                    float storedValue =  useStoredValues ? _tableMetadata.GetAnchorSize(rowId).x : 0;
                    width += storedValue != 0 ? storedValue : row.Value.x;
                }
                else
                {
                    float storedValue = useStoredValues ? _tableMetadata.GetAnchorSize(rowId).y : 0;
                    height += storedValue != 0 ? storedValue : row.Value.y;
                }
            }
            
            foreach (var column in _columnPreferredSizes)
            {
                if (_tableAttributes.rowHeaderVisibility == TableHeaderVisibility.Hidden && column.Key == 0)
                    continue;
                if (headersToIgnore != null && headersToIgnore.Contains(column.Key))
                    continue;
                
                int columnId = column.Key == 0 && _table.IsSubTable ? _table.ParentCell.Id : column.Key;
                if (transposed)
                {
                    float storedValue = useStoredValues ? _tableMetadata.GetAnchorSize(columnId).y : 0;
                    height += storedValue != 0 ? storedValue : column.Value.y;
                }
                else
                {
                    float storedValue = useStoredValues ? _tableMetadata.GetAnchorSize(columnId).x : 0;
                    width += storedValue != 0 ? storedValue : column.Value.x;
                }
            }
            
            return new Vector2(width, height);
        }
        
        public Vector2 GetHeaderSize(CellAnchor cellAnchor)
        {
            bool transposed = !_table.IsSubTable && _tableMetadata.IsTransposed;

            if (!transposed)
            {
                return cellAnchor switch
                {
                    Row row => new Vector2(_columnPreferredSizes[0].x, _rowPreferredSizes[row.Id].y),
                    Column column => new Vector2(_columnPreferredSizes[column.Id].x, _rowPreferredSizes[0].y),
                    _ => new Vector2(_columnPreferredSizes[0].x, _rowPreferredSizes[0].y)
                };
            }
            
            return cellAnchor switch
            {
                Row row => new Vector2(_rowPreferredSizes[row.Id].x, _columnPreferredSizes[0].y),
                Column column => new Vector2(_rowPreferredSizes[0].x, _columnPreferredSizes[column.Id].y),
                _ => new Vector2(_rowPreferredSizes[0].x, _columnPreferredSizes[0].y)
            };
        }
        
        public Vector2 GetCellSize(Cell cell, bool transposed)
        {
            return transposed ? 
                new Vector2(_rowPreferredSizes[cell.row.Id].x, _columnPreferredSizes[cell.column.Id].y)
                : new Vector2(_columnPreferredSizes[cell.column.Id].x, _rowPreferredSizes[cell.row.Id].y);
        }

        public void StoreCellSizeInMetadata(Cell cell)
        {
            StoreRowSizeInMetadata(cell.row);
            StoreColumnSizeInMetadata(cell.column);
        }
        
        public void StoreHeaderSizeInMetadata(CellAnchor cellAnchor)
        {
            switch (cellAnchor)
            {
                case Row row:
                    StoreRowSizeInMetadata(row);
                    break;
                case Column column:
                    StoreColumnSizeInMetadata(column);
                    break;
            }
        }
        
        public void AddHeaderSize(CellAnchor cellAnchor, Vector2 size)
        {
            switch (cellAnchor)
            {
                case Row row:
                    AddColumnSize(0, row.Id, size);
                    AddRowSize(row.Id, 0, size);
                    break;
                case Column column:
                    AddRowSize(0, column.Id, size);
                    AddColumnSize(column.Id, 0, size);
                    break;
                default:
                    AddRowSize(0, 0,size);
                    AddColumnSize(0,0, size);
                    break;
            }
        }
        
        public void AddCellSize(Cell cell, Vector2 size)
        {
            AddRowSize(cell.row.Id, cell.Id, size);
            AddColumnSize(cell.column.Id, cell.Id, size);
        }
        
        public void RemoveHeaderSize(CellAnchor cellAnchor)
        {
            switch (cellAnchor)
            {
                case Row row:
                    RemoveColumnSize(0, row.Id);
                    RemoveRowSize(row.Id, 0);
                    break;
                case Column column:
                    RemoveRowSize(0, column.Id);
                    RemoveColumnSize(column.Id, 0);
                    break;
                default:
                    RemoveRowSize(0, 0);
                    RemoveColumnSize(0, 0);
                    break;
            }
        }

        public void RemoveColumnSize(int columnId)
        {
            RemoveRowSize(0, columnId);
            _columnSizes.Remove(columnId);
            _columnPreferredSizes.Remove(columnId);
        }
        
        public void RemoveRowSize(int rowId)
        {
            RemoveColumnSize(0, rowId);
            _rowSizes.Remove(rowId);
            _rowPreferredSizes.Remove(rowId);
        }

        private void StoreColumnSizeInMetadata(Column column)
        {
            if(column.Table != _table) return;
            Vector2 size = _tableMetadata.GetAnchorSize(column.Id);

            if (!_table.IsSubTable && _tableMetadata.IsTransposed)
            {
                size.y = GetHeaderSize(column).y;
            }
            else
            {
                size.x = GetHeaderSize(column).x;
            }
            
            _tableMetadata.SetAnchorSize(column.Id, size);
        }
        
        private void StoreRowSizeInMetadata(Row row)
        {
            if(row.Table != _table) return;
            Vector2 size = _tableMetadata.GetAnchorSize(row.Id);

            if (!_table.IsSubTable && _tableMetadata.IsTransposed)
            {
                size.x = GetHeaderSize(row).x;
            }
            else
            {
                size.y = GetHeaderSize(row).y;
            }
            
            _tableMetadata.SetAnchorSize(row.Id, size);
        }
        
        private void AddColumnSize(int columnId, int cellId, Vector2 size)
        {
           AddAnchorSize(columnId, cellId, size, _columnSizes, _columnPreferredSizes);
        }
        
        private void RemoveColumnSize(int columnId, int cellId)
        {
            RemoveAnchorSize(columnId, cellId, _columnSizes, _columnPreferredSizes);
        }
        
        private void AddRowSize(int rowId, int cellId, Vector2 size)
        {
            AddAnchorSize(rowId, cellId, size, _rowSizes, _rowPreferredSizes);
        }
        
        private void RemoveRowSize(int rowId, int cellId)
        {
            RemoveAnchorSize(rowId, cellId, _rowSizes, _rowPreferredSizes);
        }
        
        private void AddAnchorSize(int anchorId, int cellId, Vector2 size, Dictionary<int, Dictionary<int, Vector2>> sizes, Dictionary<int, Vector2> preferredSizes)
        {
            if (!sizes.ContainsKey(anchorId))
                sizes[anchorId] = new Dictionary<int, Vector2>();
            
            if (sizes[anchorId].TryAdd(cellId, size))
            {
                if(!preferredSizes.TryAdd(anchorId, size))
                    preferredSizes[anchorId] = new Vector2(Mathf.Max(preferredSizes[anchorId].x, size.x), Mathf.Max(preferredSizes[anchorId].y, size.y));
            }
            else
            {
                sizes[anchorId][cellId] = size;
                Vector2 newSize = sizes[anchorId].Values.Aggregate(Vector2.zero, Vector2.Max);
                preferredSizes[anchorId] = newSize;
            }
        }
        
        private void RemoveAnchorSize(int anchorId, int cellId, Dictionary<int, Dictionary<int, Vector2>> sizes, Dictionary<int, Vector2> preferredSizes)
        {
            if(!sizes.ContainsKey(anchorId) || !sizes[anchorId].ContainsKey(cellId)) return;
            
            sizes[anchorId].Remove(cellId);
            preferredSizes[anchorId] = sizes[anchorId].Values.Aggregate(Vector2.zero, Vector2.Max);
        }
    }
}