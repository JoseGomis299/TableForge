using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        public event Action OnSelectionChanged;
        
        public bool SelectionEnabled {get; set;}
        HashSet<Cell> SelectedCells { get; }
        HashSet<CellAnchor> SelectedAnchors { get; }
    }
}