using System.Collections.Generic;
using System.Linq;
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
        Cell Preselect(CellSelector selector, PointerDownEvent evt, List<Cell> cellsAtPosition);
    }

    /// <summary>
    /// Factory that returns the proper selection strategy based on modifier keys.
    /// </summary>
    internal static class SelectionStrategyFactory
    {
        public static ISelectionStrategy GetSelectionStrategy(PointerDownEvent evt)
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
        public Cell Preselect(CellSelector selector, PointerDownEvent evt, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            foreach (var cell in cellsAtPosition)
            {
                if (selector.SelectedCells.Contains(cell))
                {
                    selector.CellsToDeselect.Add(cell);
                    lastSelectedCell = cell;
                }
                else
                {
                    selector.SelectedCells.Add(cell);
                    lastSelectedCell = cell;
                }
            }
            selector.FirstSelectedCell = selector.SelectedCells.FirstOrDefault();
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
        public Cell Preselect(CellSelector selector, PointerDownEvent evt, List<Cell> cellsAtPosition)
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
                {
                    var firstRow = selector.TableControl.GetCellRow(firstCell);
                    var lastRow = selector.TableControl.GetCellRow(lastSelectedCell);
                    var firstColumn = selector.TableControl.GetCellColumn(firstCell);
                    var lastColumn = selector.TableControl.GetCellColumn(lastSelectedCell);
                    var cells = CellLocator.GetCellRange(selector.TableControl,
                                                         firstRow.Id, firstColumn.Id,
                                                         lastRow.Id, lastColumn.Id);
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
        public Cell Preselect(CellSelector selector, PointerDownEvent evt, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            // Mark all currently selected cells for deselection.
            selector.CellsToDeselect = new HashSet<Cell>(selector.SelectedCells);
            selector.FirstSelectedCell = cellsAtPosition.FirstOrDefault();
            if (cellsAtPosition.Count == 1)
            {
                lastSelectedCell = selector.FirstSelectedCell;
            }
            foreach (var cell in cellsAtPosition)
            {
                selector.SelectedCells.Add(cell);
                selector.CellsToDeselect.Remove(cell);
                lastSelectedCell = cell;
            }
            return lastSelectedCell;
        }
    }
}
