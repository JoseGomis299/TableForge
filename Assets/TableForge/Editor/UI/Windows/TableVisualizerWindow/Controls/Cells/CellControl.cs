using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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
        
        public void Refresh()
        {
            Cell.RefreshData();
            OnRefresh();
        }
        
        public virtual void Refresh(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            Refresh();

            IsSelected = tableControl.CellSelector.IsCellSelected(cell);
        }

        protected abstract void OnRefresh();
        
        protected void SetPreferredSize(float width, float height)
        {
            TableControl.PreferredSize.AddCellSize(Cell, new Vector2(width, height));
        }
        
        protected virtual void SetCellValue(object value)
        {
            if (Cell.GetValue().Equals(value)) return;
            
            SetCellValueCommand command = new SetCellValueCommand(Cell, this, Cell.GetValue(), value);
            if(UndoRedoManager.GetLastUndoCommand() is SetCellValueCommand lastCommand && lastCommand.Cell.Id == Cell.Id)
            {
                lastCommand.Combine(command);
            }
            else UndoRedoManager.Do(command);
        }

        public void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(Cell, TableControl.Metadata);
            SetPreferredSize(size.x, size.y);
        }
    }
}