using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class CellControl : VisualElement
    {
        private bool _hasFunction;
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                {
                    this.SetImmediateChildrenEnabled(false);
                    LowerOverlay.style.visibility = Visibility.Hidden;
                }
                else
                {
                    this.SetImmediateChildrenEnabled(true);
                    LowerOverlay.style.visibility = Visibility.Visible;
                }

                _isSelected = value;
            }
        }
        
        private bool HasFunction
        {
            get => _hasFunction;
            set
            {
                _hasFunction = value;
                if (_hasFunction)
                {
                    if (TableControl.FunctionExecutor.IsCellFunctionCorrect(Cell.Id))
                    { 
                        RemoveFromClassList(TableVisualizerUss.CellWithIncorrectFunction);
                        AddToClassList(TableVisualizerUss.CellWithFunction);
                    }
                    else
                    {
                        RemoveFromClassList(TableVisualizerUss.CellWithFunction);
                        AddToClassList(TableVisualizerUss.CellWithIncorrectFunction);
                    }
                }
                else
                {
                    RemoveFromClassList(TableVisualizerUss.CellWithFunction);
                    RemoveFromClassList(TableVisualizerUss.CellWithIncorrectFunction);
                }
            }
        }
        
        public VisualElement LowerOverlay { get; }

        public TableControl TableControl { get; private set; }
        public Cell Cell { get; protected set; }

        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            LowerOverlay = new VisualElement { name = "lower-overlay" };
           
            AddToClassList(TableVisualizerUss.TableCell);
        }

        public void OnCreationComplete()
        {
            LowerOverlay.AddToClassList(TableVisualizerUss.CellOverlay);
            Insert(0, LowerOverlay);
        }
        
        public void Refresh()
        {
            IsSelected = TableControl.CellSelector.IsCellSelected(Cell);
            HasFunction = !string.IsNullOrEmpty(TableControl.Metadata.GetFunction(Cell.Id)?.Trim());

            Cell.RefreshData();
            OnRefresh();
        }
        
        public virtual void Refresh(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            Refresh();
        }

        protected abstract void OnRefresh();
        
        protected void SetPreferredSize(float width, float height)
        {
            TableControl.PreferredSize.AddCellSize(Cell, new Vector2(width, height));
        }
        
        protected virtual void SetCellValue(object value)
        {
            if (Cell.GetValue() != null && Cell.GetValue().Equals(value)) return;
            
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