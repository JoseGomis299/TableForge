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
                    this.SetImmediateChildrenEnabled(false);
                    RemoveFromClassList(USSClasses.Selected);
                }
                else
                {
                    this.SetImmediateChildrenEnabled(true);
                    AddToClassList(USSClasses.Selected);
                }

                _isSelected = value;
            }
        }
        public bool IsVisible => TableControl.ColumnHeaders[TableControl.GetCellColumn(Cell).Id].IsVisible && TableControl.RowHeaders[TableControl.GetCellRow(Cell).Id].IsVisible;
        public TableControl TableControl { get; private set; }
        public Cell Cell { get; protected set; }

        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            IsSelected = tableControl.CellSelector.SelectedCells.Contains(Cell);
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
        
        public virtual void Refresh(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            Refresh();

            IsSelected = tableControl.CellSelector.SelectedCells.Contains(cell);
        }
        
        protected void SetPreferredSize(float width, float height)
        {
            TableControl.PreferredSize.AddCellSize(Cell, new Vector2(width, height));
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
                RecalculateSize();
            }
        }
        
        protected virtual void SetCellValue(object value)
        {
            Cell.SetValue(value);
        }

        protected void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(Cell, TableControl.Metadata);
            SetPreferredSize(size.x, size.y);
        }
    }
}