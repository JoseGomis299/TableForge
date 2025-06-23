using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal interface ICellSelector
    {
        event Action OnSelectionChanged;
        event Action OnFocusedCellChanged;
        
        bool SelectionEnabled {get; set;}
        bool IsCellSelected(Cell cell);
        bool IsAnchorSelected(CellAnchor cellAnchor);
        bool IsAnchorSubSelected(CellAnchor cellAnchor);
        bool IsCellFocused(Cell cell);
        void ClearSelection();
        void ClearSelection(Table fromTable);
        List<Row> GetSelectedRows(Table fromTable);
        List<Column> GetSelectedColumns(Table fromTable);
        List<Cell> GetSelectedCells(Table fromTable);
        void RemoveRowSelection(Row row);
        void SetSelection(List<Cell> newSelection, bool setFocused = true);
        void SetFocusedCell(Cell cell);
        Cell GetFocusedCell();
    }
}