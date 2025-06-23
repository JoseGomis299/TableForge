using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// - Clears previous selection and selects only the clicked cell(s).
    /// </summary>
    internal class DefaultSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(PreselectArguments args)
        {
            var selector = args.Selector;
            var cellsAtPosition = args.CellsAtPosition;
            var selectedAnchors = args.SelectedAnchors;

            if (selector.IsCellSelected(cellsAtPosition.FirstOrDefault()) && args.RightClicked)
            {
                return cellsAtPosition.Last();
            }
            
            Cell lastSelectedCell = null;
            // Mark all currently selected cells for deselection.
            selector.CellsToDeselect = new HashSet<Cell>(selector.SelectedCells);
            selector.AnchorsToDeselect = new HashSet<CellAnchor>(selector.SelectedAnchors);
            selector.AnchorsToDeselect.ExceptWith(selectedAnchors);
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
