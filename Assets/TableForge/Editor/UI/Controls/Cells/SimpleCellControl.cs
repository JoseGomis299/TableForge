using TableForge.Exceptions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class SimpleCellControl : CellControl
    {
        private VisualElement _field;
        
        protected VisualElement Field
        {
            get => _field;
            set
            {
                _field = value;
                _field.RegisterCallback<FocusInEvent>(_ =>
                {
                    OnFocusIn();
                });
            }
        }

        protected SimpleCellControl(Cell cell, TableControl tableControl) : base(cell, tableControl)
        {
          
        }

        protected virtual void OnFocusIn()
        {
            var focusedCell = TableControl.CellSelector.FocusedCell;
            var rootTableControl = TableControl.GetRootTableControl();
            
            if (focusedCell != null)
            {
                var rootCell = focusedCell.GetRootCell();
                var rootRowHeaderControl = rootTableControl.GetRowHeaderControl(rootCell.Row);
                var rootColumnHeaderControl = rootTableControl.GetColumnHeaderControl(rootCell.Column);
                
                rootTableControl.RowVisibilityManager.UnlockHeaderVisibility(rootRowHeaderControl);
                rootTableControl.ColumnVisibilityManager.UnlockHeaderVisibility(rootColumnHeaderControl);
            }
            
            rootTableControl.CellSelector.FocusedCell = Cell.GetRootCell();
            focusedCell = rootTableControl.CellSelector.FocusedCell;
            rootTableControl.RowVisibilityManager.LockHeaderVisibility(rootTableControl.GetRowHeaderControl(focusedCell.Row));
            rootTableControl.ColumnVisibilityManager.LockHeaderVisibility(rootTableControl.GetColumnHeaderControl(focusedCell.Column));
        }
    }
}