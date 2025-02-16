using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.Instant, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _subTableControl = new TableControl(tableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()));
            _subTableControl.SetTable(cell.SubTable);
            
            VisualElement container = new VisualElement();
            container.AddToClassList(USSClasses.SubTableContainer);
            container.Add(_subTableControl);
            Add(container);
            
            IsSelected = false;
        }
    }
}