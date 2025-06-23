using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// - If no cell was previously selected, the clicked cell becomes the first selection.
    /// - Otherwise, select the range from the first selected cell to the current cell.
    /// </summary>
    internal class MultipleSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(PreselectArguments args)
        {
            var selector = args.selector;
            var cellsAtPosition = args.cellsAtPosition;
            var selectedAnchors = args.selectedAnchors;
            
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
                    
                    bool selectAnchors = selectedAnchors.Count > 0 || (selectedAnchors.Count > 0 && selector.SelectedAnchors.Count > 0);
                    bool anchorIsRow = false;
                    if(selectAnchors)
                    {
                        selector.AnchorsToDeselect = new HashSet<CellAnchor>(selector.SelectedAnchors);
                        anchorIsRow = selectedAnchors.First() is Row;
                    }

                    HashSet<CellAnchor> anchorsToSelect = new HashSet<CellAnchor>();
                    foreach (var cell in cells)
                    {
                        selector.SelectedCells.Add(cell);
                        selector.CellsToDeselect.Remove(cell);

                        if (selectAnchors)
                        {
                            if (anchorsToSelect.Add(anchorIsRow ? cell.row : cell.column))
                            {
                                selector.SelectedAnchors.Add(anchorIsRow ? cell.row : cell.column);
                                selectedAnchors.Add(anchorIsRow ? cell.row : cell.column);
                            }
                        }
                    }
                    
                    selector.AnchorsToDeselect.ExceptWith(selectedAnchors);
                }
            }
            return lastSelectedCell;
        }
    }
}