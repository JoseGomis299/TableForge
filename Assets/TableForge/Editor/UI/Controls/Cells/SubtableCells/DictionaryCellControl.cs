using UnityEngine.UIElements;

namespace TableForge.UI
{
    [CellControlUsage(typeof(DictionaryCell), CellSizeCalculationMethod.AutoSize)] 
    [SubTableCellControlUsage(TableType.Dynamic, TableReorderMode.None, TableHeaderVisibility.Hidden, TableHeaderVisibility.ShowHeaderName)]
    internal class DictionaryCellControl : SubTableCellControl
    {
        private TableControl _subTableControl;
        public DictionaryCellControl(DictionaryCell cell, TableControl tableControl) : base(cell, tableControl)
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