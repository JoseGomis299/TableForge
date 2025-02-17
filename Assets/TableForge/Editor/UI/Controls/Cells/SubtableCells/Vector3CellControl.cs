using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(Vector3Cell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class Vector3CellControl : SubTableCellControl
    {
        public Vector3CellControl(Vector3Cell cell, TableControl tableControl) : base(cell, tableControl)
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