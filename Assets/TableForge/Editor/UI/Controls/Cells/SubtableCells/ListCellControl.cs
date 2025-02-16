using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.Instant, TableHeaderVisibility.ShowHeaderNumber, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public ListCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl)
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