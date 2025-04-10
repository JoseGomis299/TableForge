using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        event Action OnSelectionChanged;
        
        Cell FocusedCell { get; }
        bool SelectionEnabled {get; set;}
        HashSet<Cell> SelectedCells { get; }
        HashSet<CellAnchor> SelectedAnchors { get; }
        void ClearSelection();
    }
}