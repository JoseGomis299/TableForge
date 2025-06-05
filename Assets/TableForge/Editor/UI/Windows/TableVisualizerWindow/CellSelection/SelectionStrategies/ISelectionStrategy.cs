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
        Cell Preselect(PreselectArguments args);
    }
    
    internal class PreselectArguments
    {
        public CellSelector Selector;
        public List<Cell> CellsAtPosition;
        public List<CellAnchor> SelectedAnchors;
        public bool RightClicked;
        public bool ClickedOnToolbar;
        public bool DoubleClicked;

        public PreselectArguments()
        {
            CellsAtPosition = new List<Cell>();
            SelectedAnchors = new List<CellAnchor>();
        }
        
        public PreselectArguments(CellSelector selector)
        {
            Selector = selector;
            CellsAtPosition = new List<Cell>();
            SelectedAnchors = new List<CellAnchor>();
        }
    }
}