
using UnityEngine;

namespace TableForge.UI
{
    internal abstract class SubTableCellControl : CellControl
    {
        public TableControl SubTableControl { get; protected set; }

        protected SubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }
    }
}