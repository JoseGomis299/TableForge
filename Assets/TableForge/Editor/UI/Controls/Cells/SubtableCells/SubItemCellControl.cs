using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(SubItemCell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.ExplicitReorder, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class SubItemCellControl : SubTableCellControl
    {
        public SubItemCellControl(SubItemCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            SubTableControl = new TableControl(tableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this);
            SubTableControl.SetTable(cell.SubTable);
            
            VisualElement container = new VisualElement();
            container.AddToClassList(USSClasses.SubTableContainer);
            container.Add(SubTableControl);
            if(cell.GetValue() == null)
                container.Add(new NullItemAddRowControl(SubTableControl));
            Add(container);
            
            IsSelected = false;
            
            SubTableControl.HorizontalResizer.OnResize += _ => RecalculateSize();
            SubTableControl.VerticalResizer.OnResize += _ => RecalculateSize();
            InitializeSize();
        }
    }
}