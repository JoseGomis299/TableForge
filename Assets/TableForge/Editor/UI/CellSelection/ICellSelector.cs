using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        event Action OnSelectionChanged;
        
        bool SelectionEnabled {get; set;}
        HashSet<Cell> SelectedCells { get; }
        HashSet<CellAnchor> SelectedAnchors { get; }
        void ClearSelection();
    }
}