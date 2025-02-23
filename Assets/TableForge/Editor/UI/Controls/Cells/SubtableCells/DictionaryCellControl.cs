using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(DictionaryCell), CellSizeCalculationMethod.AutoSize)] 
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class DictionaryCellControl : SubTableCellControl
    {
        public DictionaryCellControl(DictionaryCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            SubTableControl = new TableControl(tableControl.Root, CellStaticData.GetSubTableCellAttributes(GetType()), this);
            SubTableControl.SetTable(cell.SubTable);
            
            VisualElement container = new VisualElement();
            container.AddToClassList(USSClasses.SubTableContainer);
            container.Add(SubTableControl);
            Add(container);
            
            IsSelected = false;
            
            SubTableControl.HorizontalResizer.OnResize += _ => RecalculateSize();
            SubTableControl.VerticalResizer.OnResize += _ => RecalculateSize();
            InitializeSize();
        }
    }
}