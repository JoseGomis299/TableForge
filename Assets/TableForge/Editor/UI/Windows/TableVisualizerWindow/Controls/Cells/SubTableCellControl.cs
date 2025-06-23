
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal abstract class SubTableCellControl : CellControl
    {
        public TableControl SubTableControl { get; protected set; }

        protected readonly TableControl parentTableControl;

        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            Cell = cell;
            parentTableControl = tableControl;
        }

        protected override void OnRefresh() { }

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