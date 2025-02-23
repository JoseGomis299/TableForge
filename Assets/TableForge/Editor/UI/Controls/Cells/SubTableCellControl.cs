
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
                
                if(!value)
                    SubTableControl?.CellSelector.ClearSelection();
            }
        }
        public TableControl SubTableControl { get; protected set; }

        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            OnRefresh = () =>
            {
                // if(SubTableControl != null && !SubTableControl.PageManager.IsCurrentPageComplete)
                //     SubTableControl?.RebuildPage();
                // else
                    SubTableControl?.RefreshPage();
            };
        }
        
        protected void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);

            SetDesiredSize(size.x, size.y);
        }
    }
}