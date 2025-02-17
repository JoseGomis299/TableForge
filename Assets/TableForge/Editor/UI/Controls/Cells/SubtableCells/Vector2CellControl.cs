using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(Vector2Cell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class Vector2CellControl : SubTableCellControl
    {
        public Vector2CellControl(Vector2Cell cell, TableControl tableControl) : base(cell, tableControl)
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