using System.Linq;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// - If a cell is already selected, mark it for deselection.
    /// - Otherwise, add the cell.
    /// </summary>
    internal class ToggleSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(PreselectArguments args)
        {
            var selector = args.selector;
            var cellsAtPosition = args.cellsAtPosition;

            Cell lastSelectedCell = null;

            if (cellsAtPosition.Count == 1)
            {
                var cell = cellsAtPosition.First();
                
                if(selector.SelectedCells.Contains(cell))
                {
                    selector.CellsToDeselect.Add(cell);
                    foreach (var descendant in cell.GetDescendants())
                    {
                        selector.CellsToDeselect.Add(descendant);
                    }
               
                    if (cell == selector.FocusedCell)
                    {
                        selector.FocusedCell = selector.SelectedCells.FirstOrDefault(x => !selector.CellsToDeselect.Contains(x));
                    }
                }
                else
                {
                    selector.FocusedCell = cell;
                }
            }
            else if (cellsAtPosition.Count > 1)
            {
                bool focusedCellAtPosition = false;
                bool allSelectedAtPosition = true;
                
                foreach (var cell in cellsAtPosition)
                {
                    if (selector.SelectedCells.Add(cell))
                    {
                        allSelectedAtPosition = false;
                    }

                    if(cell == selector.FocusedCell)
                    {
                        focusedCellAtPosition = true;
                    }
                    lastSelectedCell = cell;
                }
                
                if (allSelectedAtPosition && !focusedCellAtPosition)
                {
                    foreach (var cell in cellsAtPosition)
                    {
                        selector.CellsToDeselect.Add(cell);
                        foreach (var descendant in cell.GetDescendants())
                        {
                            selector.CellsToDeselect.Add(descendant);
                        }
                    }
                }
                else if(!focusedCellAtPosition)
                    selector.FocusedCell = cellsAtPosition.First();
            }

            return lastSelectedCell;
        }
    }
}