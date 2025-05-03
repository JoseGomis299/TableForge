using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    /// <summary>
    /// - If no cell was previously selected, the clicked cell becomes the first selection.
    /// - Otherwise, select the range from the first selected cell to the current cell.
    /// </summary>
    internal class MultipleSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            if (selector.SelectedCells.Count == 0)
            {
                selector.FocusedCell = cellsAtPosition.FirstOrDefault();
                foreach (var cell in cellsAtPosition)
                {
                    selector.SelectedCells.Add(cell);
                }
                lastSelectedCell = selector.FocusedCell;
            }
            else
            {
                var firstCell = selector.FocusedCell;
                lastSelectedCell = cellsAtPosition.LastOrDefault();
                
                if (firstCell != null && lastSelectedCell != null)
                {              
                    var cells = CellLocator.GetCellRange(firstCell, lastSelectedCell, selector.TableControl);
                    // Mark cells outside the new range for deselection.
                    selector.CellsToDeselect = new HashSet<Cell>(selector.SelectedCells);
                    foreach (var cell in cells)
                    {
                        selector.SelectedCells.Add(cell);
                        selector.CellsToDeselect.Remove(cell);
                    }
                }
            }
            return lastSelectedCell;
        }
    }
}