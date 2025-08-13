
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

        protected override void OnRefresh()
        {
            if(SubTableControl == null) return;
            
            if (SubTableControl.TableData != ((SubTableCell)Cell).SubTable)
            {
                SubTableControl.SetTable(((SubTableCell)Cell).SubTable, useCachedSize:false);
            }
            else SubTableControl.Update(true);
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

        public void ClearSubTable()
        {
            SubTableControl?.ClearTable();
        }
    }
}