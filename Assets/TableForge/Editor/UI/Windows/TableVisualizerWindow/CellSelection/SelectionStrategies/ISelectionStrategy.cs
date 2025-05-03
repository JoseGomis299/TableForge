using System.Collections.Generic;

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
}