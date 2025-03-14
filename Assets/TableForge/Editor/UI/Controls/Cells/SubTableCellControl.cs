
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
            
            OnRefresh = () =>
            {
                // if(SubTableControl != null && !SubTableControl.PageManager.IsCurrentPageComplete)
                //     SubTableControl?.RebuildPage();
                // else
                    SubTableControl?.RefreshPage();
            };
        }
        
        protected virtual void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);

            SetDesiredSize(size.x, size.y);
        }
    }
}