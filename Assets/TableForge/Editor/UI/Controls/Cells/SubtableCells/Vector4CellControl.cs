using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(Vector4Cell), CellSizeCalculationMethod.AutoSize)]
    [SubTableCellControlUsage(TableType.Static, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class Vector4CellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public Vector4CellControl(Vector4Cell cell, TableControl tableControl) : base(cell, tableControl)
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