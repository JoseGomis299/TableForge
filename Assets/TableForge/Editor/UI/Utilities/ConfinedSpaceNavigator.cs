using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal class ConfinedSpaceNavigator : ICellNavigator
    {
        private readonly List<Cell> _cells = new();
        private int _currentIndex;
        
        public IReadOnlyList<Cell> Cells => _cells;
        
        public ConfinedSpaceNavigator(IReadOnlyList<Cell> cells, TableMetadata metadata, Cell focusedCell)
        {
            if (cells == null || cells.Count == 0)
                return;
        
            Dictionary<int, List<Cell>> cellGroups = new Dictionary<int, List<Cell>>();
            List<Cell> sortedCells = cells.OrderBy(cell => cell.GetDepth()).ToList();
            HashSet<int> pointedGroups = new HashSet<int>();
            LinkedList<int> notPointedGroups = new LinkedList<int>();

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
                    pointedGroups.Add(cell.Id);
                }
                else
                {
                    int parentId = cell.Table.ParentCell.Id;
                    if (!cellGroups.ContainsKey(parentId))
                    {
                        cellGroups[parentId] = new List<Cell>();
                    }
                    cellGroups[parentId].Add(cell);
                    pointedGroups.Add(cell.Id);
                }
            }
            
            //Get the groups that are not pointed by any cell.
            foreach (var key in cellGroups.Keys)
            {
                if (!pointedGroups.Contains(key))
                {
                    notPointedGroups.AddLast(key);
                }
            }
        
            // Sort the groups based on its position.
            foreach (var key in cellGroups.Keys.ToList())
            {
                cellGroups[key] = metadata.IsTableTransposed(key)
                    ? cellGroups[key].OrderBy(c => c.column.Position).ThenBy(c => c.row.Position).ToList()
                    : cellGroups[key].OrderBy(c => c.row.Position).ThenBy(c => c.column.Position).ToList();
            }

            // Clear the current cells and add the sorted cells.
            _cells.Clear();
            foreach (var key in notPointedGroups)
            {
                AddCellsFromId(key, cellGroups);
            }
        
            //Set the current index to the focused cell.
            if (focusedCell != null)
            {
                _currentIndex = _cells.IndexOf(focusedCell);
                if (_currentIndex == -1)
                    _currentIndex = 0;
            }
            else _currentIndex = 0;
        }
        
        private void AddCellsFromId(int id, Dictionary<int, List<Cell>> groups)
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

        public void SetCurrentCell(Cell cell)
        {
            if (_cells == null || _cells.Count == 0 || cell == _cells[_currentIndex])
                return;

            int index = _cells.IndexOf(cell);
            _currentIndex = index != -1 ? index : 0;
        }

        public Cell GetCurrentCell()
        {
            if (_cells == null || _cells.Count == 0)
                return null;

            return _cells[_currentIndex];
        }
    }
}