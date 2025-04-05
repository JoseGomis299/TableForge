using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SimpleCellControl : CellControl, ISimpleCellControl
    {
        private VisualElement _field;
        
        protected VisualElement Field
        {
            get => _field;
            set
            {
                _field = value;
                _field.focusable = false;
                _field.RegisterCallback<FocusInEvent>(_ =>
                {
                    OnFocusIn();
                });
                _field.RegisterCallback<FocusOutEvent>(_ =>
                {
                   BlurField();
                });
            }
        }
        
        protected SimpleCellControl(Cell cell, TableControl tableControl) : base(cell, tableControl)
        {
        }
        
        public virtual void FocusField()
        {
            if (Field == null) return;
            
            Field.focusable = true;
            Field.Focus();
        }
        
        public virtual void BlurField()
        {
            if (Field == null) return;
            
            Field.focusable = false;
            Field.Blur();
        }
        
        public bool IsFieldFocused()
        {
           return Field?.focusController?.focusedElement == Field;
        }

        private void OnFocusIn()
        {
            var focusedCell = TableControl.CellSelector.FocusedCell;
            var rootTableControl = TableControl.GetRootTableControl();
            
            if (focusedCell != null)
            {
                var rootCell = focusedCell.GetRootCell();
                var rootRowHeaderControl = rootTableControl.GetRowHeaderControl(rootTableControl.GetCellRow(rootCell));
                var rootColumnHeaderControl = rootTableControl.GetColumnHeaderControl(rootTableControl.GetCellColumn(rootCell));
                
                rootTableControl.RowVisibilityManager.UnlockHeaderVisibility(rootRowHeaderControl);
                rootTableControl.ColumnVisibilityManager.UnlockHeaderVisibility(rootColumnHeaderControl);
            }
            
            rootTableControl.CellSelector.FocusedCell = Cell.GetRootCell();
            rootTableControl.CellSelector.FirstSelectedCell = Cell;
            focusedCell = rootTableControl.CellSelector.FocusedCell;
            rootTableControl.RowVisibilityManager.LockHeaderVisibility(rootTableControl.GetRowHeaderControl(rootTableControl.GetCellRow(focusedCell)));
            rootTableControl.ColumnVisibilityManager.LockHeaderVisibility(rootTableControl.GetColumnHeaderControl(rootTableControl.GetCellColumn(focusedCell)));
        }
    }
}