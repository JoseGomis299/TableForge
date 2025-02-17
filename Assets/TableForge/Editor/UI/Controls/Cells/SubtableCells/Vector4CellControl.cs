using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(Vector4Cell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class Vector4CellControl : SubTableCellControl
    {
        public Vector4CellControl(Vector4Cell cell, TableControl tableControl) : base(cell, tableControl)
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