using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
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
        private static ToggleSelectionStrategy _toggleSelectionStrategy;
        private static MultipleSelectionStrategy _multipleSelectionStrategy;
        private static DefaultSelectionStrategy _defaultSelectionStrategy;
        
        public static ISelectionStrategy GetSelectionStrategy(IMouseEvent evt)
        {
            if (evt.ctrlKey) 
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if (evt.shiftKey)
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }

        public static ISelectionStrategy GetSelectionStrategy<T>() where T :ISelectionStrategy
        {
            if(typeof(T) == typeof(ToggleSelectionStrategy))
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if(typeof(T) == typeof(MultipleSelectionStrategy))
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }
        
        public static ISelectionStrategy GetSelectionStrategy(bool isCtrl, bool isShift)
        {
            if (isCtrl) 
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if (isShift)
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }
    }

    /// <summary>
    /// - If a cell is already selected, mark it for deselection.
    /// - Otherwise, add the cell.
    /// </summary>
    internal class ToggleSelectionStrategy : ISelectionStrategy
    {
        public Cell Preselect(CellSelector selector, List<Cell> cellsAtPosition)
        {
            Cell lastSelectedCell = null;
            bool focusedCellHasChanged = false;
            
            foreach (var cell in cellsAtPosition)
            {
                if (selector.SelectedCells.Add(cell))
                {
                    lastSelectedCell = cell;
                    selector.FocusedCell = cell;
                }
                else
                {
                    if(cellsAtPosition.Count > 1) continue;
                    
                    selector.CellsToDeselect.Add(cell);
                    foreach (var descendant in cell.GetDescendants())
                    {
                        selector.CellsToDeselect.Add(descendant);
                    }
                    
                    lastSelectedCell = cell;
                    if(!focusedCellHasChanged && cell == selector.FocusedCell)
                    {
                        focusedCellHasChanged = true;
                    }
                }
            }
            
            if(focusedCellHasChanged)
                selector.FocusedCell = selector.SelectedCells.FirstOrDefault(x => !selector.CellsToDeselect.Contains(x));
            
            return lastSelectedCell;
        }
    }

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
