using System;
using System.Collections;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal interface ICellSelector
    {
        event Action OnSelectionChanged;
        
        bool SelectionEnabled {get; set;}
        bool IsCellSelected(Cell cell);
        bool IsAnchorSelected(CellAnchor cellAnchor);
        bool IsAnchorSubSelected(CellAnchor cellAnchor);
        bool IsCellFocused(Cell cell);
        void ClearSelection();
        void ClearSelection(Table fromTable);
        List<Row> GetSelectedRows();
        List<Column> GetSelectedColumns();
        void RemoveRowSelection(Row row);
    }
}