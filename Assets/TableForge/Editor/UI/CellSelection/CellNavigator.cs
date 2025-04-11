using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    internal class CellNavigator
    {
        private readonly List<Cell> _cells = new();
        private int _currentIndex;
        
        public void SetNavigationSpace(IReadOnlyList<Cell> cells, TableMetadata metadata, Cell focusedCell)
        {
            if (cells == null || cells.Count == 0)
                return;
        
            var cellGroups = new Dictionary<int, List<Cell>>();
            var sortedCells = cells.OrderBy(cell => TableForge.CellExtension.GetDepth(cell)).ToList();

            // Create groups for cells.
            foreach (var cell in sortedCells)
            {
                if (cell is SubTableCell)
                {
                    if (!cellGroups.ContainsKey(cell.Id))
                    {
                        cellGroups[cell.Id] = new List<Cell>();
                    }
                }

                if (!cell.Table.IsSubTable)
                {
                    if (!cellGroups.ContainsKey(0))
                    {
                        cellGroups[0] = new List<Cell>();
                    }
                    cellGroups[0].Add(cell);
                }
                else
                {
                    int parentId = cell.Table.ParentCell.Id;
                    if (!cellGroups.ContainsKey(parentId))
                    {
                        cellGroups[parentId] = new List<Cell>();
                    }
                    cellGroups[parentId].Add(cell);
                }
            }
        
            // Sort the groups based on its position.
            foreach (var key in cellGroups.Keys.ToList())
            {
                cellGroups[key] = metadata.IsTableTransposed(key)
                    ? cellGroups[key].OrderBy(c => c.Column.Position).ThenBy(c => c.Row.Position).ToList()
                    : cellGroups[key].OrderBy(c => c.Row.Position).ThenBy(c => c.Column.Position).ToList();
            }

            // Clear the current cells and add the sorted cells.
            _cells.Clear();
            AddCellsFromId(0, cellGroups);
        
            //Set the current index to the focused cell.
            if (focusedCell != null)
            {
                _currentIndex = _cells.IndexOf(focusedCell);
                if (_currentIndex == -1)
                    _currentIndex = 0;
            }
            else _currentIndex = 0;
        
            void AddCellsFromId(int id, Dictionary<int, List<Cell>> groups)
            {
                if (!groups.TryGetValue(id, out var group))
                    return;

                foreach (var cell in group)
                {
                    _cells.Add(cell);
                    if (cell is SubTableCell subTableCell)
                    {
                        AddCellsFromId(subTableCell.Id, groups);
                    }
                }
            }
        }
        
        public void Clear()
        {
            _cells.Clear();
            _currentIndex = 0;
        }

        public Cell GetNextCell(int orientation)
        {
            if (_cells == null || _cells.Count == 0)
                return null;

            _currentIndex += orientation;
            
            if (_currentIndex >= _cells.Count)
                _currentIndex = 0;
            else if (_currentIndex < 0)
                _currentIndex = _cells.Count - 1;
            
            return _cells[_currentIndex];
        }
    }
}