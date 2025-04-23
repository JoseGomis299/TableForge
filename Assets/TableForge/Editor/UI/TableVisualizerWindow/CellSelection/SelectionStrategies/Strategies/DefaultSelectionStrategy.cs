using System.Collections.Generic;
using System.Linq;

namespace TableForge.UI
{
    /// <summary>
    /// - Clears previous selection and selects only the clicked cell(s).
    /// </summary>
    internal class DefaultSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            // Mark all currently selected cells for deselection.
            selector.CellsToDeselect = new HashSet<Cell>(selector.SelectedCells);
            if (cellsAtPosition.Count == 1)
            {
                lastSelectedCell = selector.FocusedCell;
            }
            
            foreach (var cell in cellsAtPosition)
            {
                selector.SelectedCells.Add(cell);
                selector.CellsToDeselect.Remove(cell);

                foreach (var ascendant in cell.GetAncestors())
                {
                    selector.CellsToDeselect.Remove(ascendant);
                }
                
                lastSelectedCell = cell;
            }
            
            selector.FocusedCell = cellsAtPosition.FirstOrDefault();

            return lastSelectedCell;
        }
    }
}
