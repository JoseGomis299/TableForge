using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(DictionaryCell), CellSizeCalculationMethod.AutoSize)] 
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class DictionaryCellControl : ExpandableSubTableCellControl
    {
        public DictionaryCellControl(DictionaryCell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }
        
        protected override void InitializeSubTable()
        {
            SubTableControl = new TableControl(
                ParentTableControl.Root,
                CellStaticData.GetSubTableCellAttributes(GetType()), 
                this
            );
            
            SubTableControl.SetTable(Cell.SubTable);
            ContentContainer.Add(SubTableControl);
            
            SubTableControl.HorizontalResizer.OnResize += _ => RecalculateSize();
            SubTableControl.VerticalResizer.OnResize += _ => RecalculateSize();
            IsSelected = false;
            InitializeSize();
        }
    }
}