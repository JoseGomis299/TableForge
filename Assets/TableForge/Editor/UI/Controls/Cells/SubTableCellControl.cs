
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
                SubTableControl?.ShowScrollbars(value);
            }
        }
        public TableControl SubTableControl { get; protected set; }

        protected TableControl ParentTableControl;

        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            Cell = cell;
            ParentTableControl = tableControl;
            OnRefresh = () => SubTableControl?.RefreshPage(); 
        }
        
        protected virtual void RecalculateSizeWithCurrentValues()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);
            SetDesiredSize(size.x, size.y);
        }
    }
}