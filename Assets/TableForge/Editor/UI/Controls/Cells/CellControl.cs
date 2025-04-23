using System;
using TableForge.Exceptions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class CellControl : VisualElement
    {
        protected Action OnRefresh;
        
        private bool _isSelected;
        public bool IsSelected
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

        public TableControl TableControl { get; private set; }
        public Cell Cell { get; protected set; }

        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;

            AddToClassList(USSClasses.TableCell);
        }
        
        
        ~CellControl()
        {
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

            IsSelected = tableControl.CellSelector.IsCellSelected(cell);
        }
        
        protected void SetPreferredSize(float width, float height)
        {
            TableControl.PreferredSize.AddCellSize(Cell, new Vector2(width, height));
        }
        
        protected virtual void SetCellValue(object value)
        {
            Cell.SetValue(value);
        }

        public void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(Cell, TableControl.Metadata);
            SetPreferredSize(size.x, size.y);
        }
    }
}