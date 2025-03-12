using System;
using TableForge.Exceptions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class CellControl : VisualElement
    {
        public event Action<object> OnValueChange; 
        protected Action OnRefresh;
        
        private bool _isSelected;
        public virtual bool IsSelected
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
        public TableControl TableControl { get; }
        public Cell Cell { get; set; }

        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            AddToClassList(USSClasses.TableCell);
        }
        
        ~CellControl()
        {
            OnValueChange = null;
            OnRefresh = null;
        }
        
        public void Refresh()
        {
            Cell.RefreshData();
            OnRefresh?.Invoke();
        }
        
        protected void SetDesiredSize(float width, float height)
        {
            int columnId = TableControl.Inverted ? Cell.Row.Id : Cell.Column.Id;
            int rowId = TableControl.Inverted ? Cell.Column.Id : Cell.Row.Id;
            
            TableControl.ColumnData[columnId].AddPreferredWidth(rowId, width);
            TableControl.RowData[rowId].AddPreferredHeight(columnId, height);
        }
        
        protected void OnChange<T>(ChangeEvent<T> evt, BaseField<T> field)
        {
            try
            {
                SetCellValue(evt.newValue);
                OnValueChange?.Invoke(evt.newValue);
            }
            catch(InvalidCellValueException e)
            {
                field.SetValueWithoutNotify(evt.previousValue);
                Debug.LogWarning(e.Message);
            }
            finally
            {
                InitializeSize();
            }
        }
        
        protected virtual void SetCellValue(object value)
        {
            Cell.SetValue(value);
        }

        protected void InitializeSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(this);
            SetDesiredSize(size.x, size.y);
        }
    }
}