using System.Collections.Generic;

namespace TableForge.Editor.UI
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
        Cell Preselect(PreselectArguments args);
    }
    
    internal class PreselectArguments
    {
        public CellSelector selector;
        public List<Cell> cellsAtPosition;
        public List<CellAnchor> selectedAnchors;
        public bool rightClicked;
        public bool clickedOnToolbar;
        public bool doubleClicked;

        public PreselectArguments()
        {
            cellsAtPosition = new List<Cell>();
            selectedAnchors = new List<CellAnchor>();
        }
        
        public PreselectArguments(CellSelector selector)
        {
            this.selector = selector;
            cellsAtPosition = new List<Cell>();
            selectedAnchors = new List<CellAnchor>();
        }
    }
}