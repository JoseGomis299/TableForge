using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    /// <summary>
    /// Interface defining the selection strategy.
    /// </summary>
    internal interface ISelectionStrategy
    {
        /// <summary>
        /// Executes preselection using the given mouse event and the cells under the mouse position.
        /// Returns the "last selected cell" for further actions.
        /// </summary>
        Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition);
    }

    /// <summary>
    /// Factory that returns the proper selection strategy based on modifier keys.
    /// </summary>
    internal static class SelectionStrategyFactory
    {
        public static ISelectionStrategy GetSelectionStrategy(IMouseEvent evt)
        {
            if (evt.ctrlKey)
                return new CtrlSelectionStrategy();
            if (evt.shiftKey)
                return new ShiftSelectionStrategy();
            return new NormalSelectionStrategy();
        }
    }

    /// <summary>
    /// Strategy for CTRL key:
    /// - If a cell is already selected, mark it for deselection.
    /// - Otherwise, add the cell.
    /// </summary>
    internal class CtrlSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            bool selectedFirstCell = false;
            
            foreach (var cell in cellsAtPosition)
            {
                if (selector.SelectedCells.Add(cell))
                {
                    lastSelectedCell = cell;
                    
                    selectedFirstCell = true;
                    selector.FirstSelectedCell = cell;
                }
                else
                {
                    selector.CellsToDeselect.Add(cell);
                    foreach (var descendant in cell.GetDescendants())
                    {
                        selector.CellsToDeselect.Add(descendant);
                    }
                    
                    lastSelectedCell = cell;
                }
            }
            
            if(!selectedFirstCell)
             selector.FirstSelectedCell = selector.SelectedCells.FirstOrDefault(x => !selector.CellsToDeselect.Contains(x));
            return lastSelectedCell;
        }
    }

    /// <summary>
    /// Strategy for SHIFT key:
    /// - If no cell was previously selected, the clicked cell becomes the first selection.
    /// - Otherwise, select the range from the first selected cell to the current cell.
    /// </summary>
    internal class ShiftSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            if (selector.SelectedCells.Count == 0)
            {
                selector.FirstSelectedCell = cellsAtPosition.FirstOrDefault();
                foreach (var cell in cellsAtPosition)
                {
                    selector.SelectedCells.Add(cell);
                }
                lastSelectedCell = selector.FirstSelectedCell;
            }
            else
            {
                var firstCell = selector.FirstSelectedCell;
                lastSelectedCell = cellsAtPosition.LastOrDefault();
                
                if (firstCell != null && lastSelectedCell != null)
                {                Debug.Log($"First cell: {firstCell.GetPosition()}, last cell: {lastSelectedCell.GetPosition()}");

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

    /// <summary>
    /// Default selection strategy (no modifier keys):
    /// - Clears previous selection and selects only the clicked cell(s).
    /// </summary>
    internal class NormalSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            // Mark all currently selected cells for deselection.
            selector.CellsToDeselect = new HashSet<Cell>(selector.SelectedCells);
            if (cellsAtPosition.Count == 1)
            {
                lastSelectedCell = selector.FirstSelectedCell;
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
            
            selector.FirstSelectedCell = cellsAtPosition.FirstOrDefault();

            return lastSelectedCell;
        }
    }
}
