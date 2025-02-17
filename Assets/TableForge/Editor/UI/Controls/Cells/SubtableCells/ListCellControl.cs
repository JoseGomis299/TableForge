using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.Instant, TableHeaderVisibility.ShowHeaderNumber, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellControl : SubTableCellControl
    {
        public ListCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            SubTableControl = new TableControl(tableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()));
            SubTableControl.SetTable(cell.SubTable);

            VisualElement container = new VisualElement();
            container.AddToClassList(USSClasses.SubTableContainer);
            container.Add(SubTableControl);
            Add(container);
            
            IsSelected = false;
            InitializeSize();
        }
    }
}