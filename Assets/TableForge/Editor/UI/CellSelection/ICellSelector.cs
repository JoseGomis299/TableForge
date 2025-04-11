using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        event Action OnSelectionChanged;
        
        bool SelectionEnabled {get; set;}
        bool IsCellSelected(Cell cell);
        bool IsAnchorSelected(CellAnchor cellAnchor);
        bool IsCellFocused(Cell cell);
        void ClearSelection();
    }
}