using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        public event Action OnSelectionChanged;
        
        HashSet<Cell> SelectedCells { get; }
        void ClearSelection();
    }
}