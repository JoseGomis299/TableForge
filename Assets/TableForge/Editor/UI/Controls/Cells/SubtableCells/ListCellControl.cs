using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(ListCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.ImplicitReorder, TableHeaderVisibility.ShowHeaderNumberBase0, TableHeaderVisibility.ShowHeaderName)]
    internal class ListCellControl : SubTableCellControl
    {
        public ListCellControl(ListCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            SubTableControl = new TableControl(tableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this);
            SubTableControl.SetTable(cell.SubTable);
            
            VisualElement container = new VisualElement();
            container.AddToClassList(USSClasses.SubTableContainer);
            container.Add(SubTableControl);
            container.Add(new ListAddRowControl(SubTableControl));
            Add(container);
            
            IsSelected = false;
            
            SubTableControl.HorizontalResizer.OnResize += _ => RecalculateSize();
            SubTableControl.VerticalResizer.OnResize += _ => RecalculateSize();
            InitializeSize();
        }
    }
}