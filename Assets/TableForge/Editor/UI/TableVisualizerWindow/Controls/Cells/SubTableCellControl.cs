
using UnityEngine;

namespace TableForge.UI
{
    internal abstract class SubTableCellControl : CellControl
    {
        public TableControl SubTableControl { get; protected set; }

        protected readonly TableControl ParentTableControl;

        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            Cell = cell;
            ParentTableControl = tableControl;
            
            OnRefresh = () =>
            {
                if(SubTableControl == null) return;
                SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            }; 
        }
        
        protected virtual void RecalculateSizeWithCurrentValues()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);
            SetPreferredSize(size.x, size.y);
            
            if(TableControl.Parent is { } subTableCellControl)
            {
                subTableCellControl.RecalculateSizeWithCurrentValues();
            }
        }

        public virtual void ClearSubTable()
        {
            SubTableControl?.ClearTable();
        }
    }
}