using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class CellControl : VisualElement
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                {
                    this[0].SetEnabled(false);
                    this.RemoveFromChildrenClassList(disabledUssClassName);
                    RemoveFromClassList("selected");
                }
                else
                {
                    this[0].SetEnabled(true);
                    AddToClassList("selected");
                }

                _isSelected = value;
            }
        }
        public bool IsVisible => TableControl.ColumnHeaders[Cell.Column.Id].IsVisible && TableControl.RowHeaders[Cell.Row.Id].IsVisible;
        protected TableControl TableControl { get; }
        public Cell Cell { get; }

        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            AddToClassList("table__cell");
        }
        
        protected void SetDesiredSize(float width, float height, bool force = false)
        {
            int columnId = Cell.Column.Id;
            int rowId = Cell.Row.Id;
            
            TableControl.ColumnData[columnId].AddPreferredWidth(rowId, width);
            TableControl.RowData[rowId].AddPreferredHeight(columnId, height);
        }
        
        protected virtual void OnChange<T>(ChangeEvent<T> evt)
        {
            Cell.SetValue(evt.newValue);
            InitializeSize();
        }

        protected virtual void InitializeSize()
        {
            // Do nothing by default
        }
    }
}