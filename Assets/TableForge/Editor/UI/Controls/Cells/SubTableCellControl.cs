
using UnityEngine;

namespace TableForge.UI
{
    internal abstract class SubTableCellControl : CellControl
    {
        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                base.IsSelected = value;
                SubTableControl?.ScrollView.SetScrollbarsVisibility(value);
            }
        }
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
                // int pos = 1;
                // foreach (RowHeaderControl rowHeaderControl in SubTableControl.RowHeaders.Values)
                // {
                //     rowHeaderControl.Refresh(((SubTableCell)Cell).SubTable.Rows[pos++]);
                // }
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
    }
}