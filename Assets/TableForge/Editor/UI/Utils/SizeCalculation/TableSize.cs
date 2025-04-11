using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.UI
{
    internal class TableSize
    {
        private readonly Dictionary<string, Dictionary<string, Vector2>> _rowSizes = new();
        private readonly Dictionary<string, Dictionary<string, Vector2>> _columnSizes = new();
        private readonly Dictionary<string, Vector2> _rowPreferredSizes = new();
        private readonly Dictionary<string, Vector2> _columnPreferredSizes = new();
        
        private readonly TableMetadata _tableMetadata;
        private readonly TableAttributes _tableAttributes;
        private readonly Table _table;

        public TableSize(Table table, TableMetadata tableMetadata, TableAttributes tableAttributes)
        {
            _table = table;
            _tableMetadata = tableMetadata;
            _tableAttributes = tableAttributes;
        }

        public Vector2 GetTotalSize(bool useStoredValues)
        {
            bool transposed = !_table.IsSubTable && _tableMetadata.IsTransposed;
            float width = 0, height = 0;
            
            foreach (var row in _rowPreferredSizes)
            {
                if (_tableAttributes.ColumnHeaderVisibility == TableHeaderVisibility.Hidden && row.Key == "") 
                    continue;

                string rowId = row.Key == "" && _table.IsSubTable ? _table.ParentCell.Id : row.Key;
                if (transposed)
                {
                    float storedValue = useStoredValues ? _tableMetadata.GetAnchorSize(rowId).x : 0;
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
                if (_tableAttributes.RowHeaderVisibility == TableHeaderVisibility.Hidden && column.Key == "")
                    continue;
                
                string columnId = column.Key == "" && _table.IsSubTable ? _table.ParentCell.Id : column.Key;
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
                    Row row => new Vector2(_columnPreferredSizes[""].x, _rowPreferredSizes[row.Id].y),
                    Column column => new Vector2(_columnPreferredSizes[column.Id].x, _rowPreferredSizes[""].y),
                    _ => new Vector2(_columnPreferredSizes[""].x, _rowPreferredSizes[""].y)
                };
            }
            
            return cellAnchor switch
            {
                Row row => new Vector2(_rowPreferredSizes[row.Id].x, _columnPreferredSizes[""].y),
                Column column => new Vector2(_rowPreferredSizes[""].x, _columnPreferredSizes[column.Id].y),
                _ => new Vector2(_rowPreferredSizes[""].x, _columnPreferredSizes[""].y)
            };
        }
        
        public Vector2 GetCellSize(Cell cell, bool transposed)
        {
            return transposed ? 
                new Vector2(_rowPreferredSizes[cell.Row.Id].x, _columnPreferredSizes[cell.Column.Id].y)
                : new Vector2(_columnPreferredSizes[cell.Column.Id].x, _rowPreferredSizes[cell.Row.Id].y);
        }

        public void StoreCellSizeInMetadata(Cell cell)
        {
            StoreRowSizeInMetadata(cell.Row);
            StoreColumnSizeInMetadata(cell.Column);
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
                    AddColumnSize("", row.Id, size);
                    AddRowSize(row.Id, "", size);
                    break;
                case Column column:
                    AddRowSize("", column.Id, size);
                    AddColumnSize(column.Id, "", size);
                    break;
                default:
                    AddRowSize("", "",size);
                    AddColumnSize("","", size);
                    break;
            }
        }
        
        public void AddCellSize(Cell cell, Vector2 size)
        {
            AddRowSize(cell.Row.Id, cell.Id, size);
            AddColumnSize(cell.Column.Id, cell.Id, size);
        }
        
        public void RemoveHeaderSize(CellAnchor cellAnchor)
        {
            switch (cellAnchor)
            {
                case Row row:
                    RemoveColumnSize("", row.Id);
                    RemoveRowSize(row.Id, "");
                    break;
                case Column column:
                    RemoveRowSize("", column.Id);
                    RemoveColumnSize(column.Id, "");
                    break;
                default:
                    RemoveRowSize("", "");
                    RemoveColumnSize("", "");
                    break;
            }
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
        
        private void AddColumnSize(string columnId, string cellId, Vector2 size)
        {
           AddAnchorSize(columnId, cellId, size, _columnSizes, _columnPreferredSizes);
        }
        
        private void RemoveColumnSize(string columnId, string cellId)
        {
            RemoveAnchorSize(columnId, cellId, _columnSizes, _columnPreferredSizes);
        }
        
        private void AddRowSize(string rowId, string cellId, Vector2 size)
        {
            AddAnchorSize(rowId, cellId, size, _rowSizes, _rowPreferredSizes);
        }
        
        private void RemoveRowSize(string rowId, string cellId)
        {
            RemoveAnchorSize(rowId, cellId, _rowSizes, _rowPreferredSizes);
        }
        
        private void AddAnchorSize(string anchorId, string cellId, Vector2 size, Dictionary<string, Dictionary<string, Vector2>> sizes, Dictionary<string, Vector2> preferredSizes)
        {
            if (!sizes.ContainsKey(anchorId))
                sizes[anchorId] = new Dictionary<string, Vector2>();
            
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
        
        private void RemoveAnchorSize(string anchorId, string cellId, Dictionary<string, Dictionary<string, Vector2>> sizes, Dictionary<string, Vector2> preferredSizes)
        {
            if(!sizes.ContainsKey(anchorId) || !sizes[anchorId].ContainsKey(cellId)) return;
            
            sizes[anchorId].Remove(cellId);
            preferredSizes[anchorId] = sizes[anchorId].Values.Aggregate(Vector2.zero, Vector2.Max);
        }
    }
}