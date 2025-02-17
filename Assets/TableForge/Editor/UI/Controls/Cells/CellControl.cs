using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class CellControl : VisualElement
    {
        public event Action<object> OnValueChange; 
        
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
                    RemoveFromClassList(USSClasses.Selected);
                }
                else
                {
                    this[0].SetEnabled(true);
                    AddToClassList(USSClasses.Selected);
                    this.RemoveFromChildrenClassList(disabledUssClassName);
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
            
            AddToClassList(USSClasses.TableCell);
        }
        
        
        protected void SetDesiredSize(float width, float height)
        {
            int columnId = Cell.Column.Id;
            int rowId = Cell.Row.Id;
            
            TableControl.ColumnData[columnId].AddPreferredWidth(rowId, width);
            TableControl.RowData[rowId].AddPreferredHeight(columnId, height);
        }
        
        protected virtual void OnChange<T>(ChangeEvent<T> evt)
        {
            try
            {
                Cell.SetValue(evt.newValue);
                OnValueChange?.Invoke(evt.newValue);
            }
            finally
            {
                InitializeSize();
            }
        }

        protected virtual void InitializeSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(this);
            SetDesiredSize(size.x, size.y);
        }
    }
}