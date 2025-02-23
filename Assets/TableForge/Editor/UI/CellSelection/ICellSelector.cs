using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        HashSet<CellControl> SelectedCells { get; }
        void SelectCell(CellControl cellControl);
        void DeselectCell(CellControl cellControl);
        void SelectAll();
        void SelectAllRecursively();
        void ClearSelection();
    }
}