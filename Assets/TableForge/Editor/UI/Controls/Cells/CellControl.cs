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
        public TableControl TableControl { get; protected set; }
        public Cell Cell { get; protected set; }

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
        
        public virtual void Refresh(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            IsSelected = tableControl.CellSelector.SelectedCells.Contains(Cell);
            
            Refresh();
            InitializeSize();
        }
        
        protected void SetDesiredSize(float width, float height)
        {
            TableControl.TableSize.AddCellSize(Cell, new Vector2(width, height));
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
            Vector2 size = SizeCalculator.CalculateSize(Cell, TableControl.Metadata);
            SetDesiredSize(size.x, size.y);
        }
    }
}