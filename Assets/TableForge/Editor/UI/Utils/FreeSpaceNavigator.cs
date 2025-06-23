using System.Linq;

namespace TableForge.Editor.UI
{
    internal class FreeSpaceNavigator : ICellNavigator
    {
        private readonly TableMetadata _metadata;
        private Cell _currentCell;
        
        private readonly Table _initialTable;
        private readonly int _minRowPosition;
        private readonly int _minColumnPosition;
        
        public FreeSpaceNavigator(TableMetadata metadata, Cell currentCell)
        {
            _metadata = metadata;
            _currentCell = currentCell;
            
            _initialTable = currentCell.Table;
            bool isTransposed = !_initialTable.IsSubTable && _metadata.IsTransposed;
            
            _minRowPosition = isTransposed ? currentCell.column.Position : currentCell.row.Position;
            _minColumnPosition = isTransposed ? currentCell.row.Position : currentCell.column.Position;
            
            while(_currentCell is SubTableCell subTableCell and not ICollectionCell && subTableCell.SubTable.Rows.Any())
            {
                _currentCell = subTableCell.SubTable.GetFirstCell();
            }
        }

        public Cell GetNextCell(int orientation)
        {
            if (_currentCell == null)
                return null;
            
            Table table = _currentCell.Table;
            bool isTransposed = !table.IsSubTable && _metadata.IsTransposed;
            
            int columnCount = isTransposed ? table.Rows.Count : table.Columns.Count;
            int rowCount = isTransposed ? table.Columns.Count : table.Rows.Count;
            int nextColumn = isTransposed ? _currentCell.row.Position + orientation : _currentCell.column.Position + orientation;
            int nextRow = isTransposed ? _currentCell.column.Position : _currentCell.row.Position;
            int minColumn = table == _initialTable ? _minColumnPosition : 1;
            
            if (nextColumn < minColumn || nextColumn > columnCount)
            {
                if (table.IsSubTable && table != _initialTable)
                {
                    _currentCell = table.ParentCell;
                    return GetNextCell(orientation);
                }

                nextColumn = nextColumn < minColumn ? columnCount : minColumn;
                
                nextRow += orientation;
                nextRow = nextRow < _minRowPosition ? rowCount : _minRowPosition;
            }

            Cell targetCell = isTransposed ?  table.GetCell(nextRow, nextColumn) : table.GetCell(nextColumn, nextRow);
            
            while(targetCell is SubTableCell subTableCell and not ICollectionCell && subTableCell.SubTable.Rows.Any())
            {
                targetCell = subTableCell.SubTable.GetFirstCell();
            }
            
            _currentCell = targetCell;
            return _currentCell;
        }

        public void SetCurrentCell(Cell cell)
        {
            if (cell == null)
                return;
            
            _currentCell = cell;
        }

        public Cell GetCellAtNextRow(int orientation)
        {
            if (_currentCell == null)
                return null;
            
            while (_currentCell.Table.IsSubTable && _currentCell.Table != _initialTable)
            {
                _currentCell = _currentCell.Table.ParentCell;
            }
            
            Table table = _currentCell.Table;
            bool isTransposed = !table.IsSubTable && _metadata.IsTransposed;
            
            int rowCount = isTransposed ? table.Columns.Count : table.Rows.Count;
            int nextRow = isTransposed ? _currentCell.column.Position + orientation : _currentCell.row.Position + orientation;
            int nextColumn = _minColumnPosition;

            if (nextRow < _minRowPosition)
            {
                nextRow = rowCount;
            }
            else if (nextRow > rowCount)
            {
                nextRow = _minRowPosition;
            }
            
            Cell targetCell = isTransposed ?  table.GetCell(nextRow, nextColumn) : table.GetCell(nextColumn, nextRow);
            
            while(targetCell is SubTableCell subTableCell and not ICollectionCell && subTableCell.SubTable.Rows.Any())
            {
                targetCell = subTableCell.SubTable.GetFirstCell();
            }
            
            _currentCell = targetCell;
            return _currentCell;
        }
        
        public Cell GetCurrentCell()
        {
            return _currentCell;
        }
    }
}