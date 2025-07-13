using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Abstract base class for cell controls in the table visualizer.
    /// Handles cell selection, function display, and value management.
    /// </summary>
    internal abstract class CellControl : VisualElement
    {
        #region Private Fields

        private bool _hasFunction;
        private bool _isSelected;

        #endregion

        #region Public Properties

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
        
        public VisualElement LowerOverlay { get; }
        public TableControl TableControl { get; private set; }
        public Cell Cell { get; protected set; }

        #endregion

        #region Private Properties

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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the CellControl class.
        /// </summary>
        /// <param name="cell">The cell data to associate with this control.</param>
        /// <param name="tableControl">The table control that owns this cell.</param>
        protected CellControl(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            LowerOverlay = new VisualElement { name = "lower-overlay" };
           
            AddToClassList(TableVisualizerUss.TableCell);
        }

        #endregion

        #region Public Methods - Lifecycle

        /// <summary>
        /// Called when the cell creation is complete.
        /// Sets up the lower overlay element.
        /// </summary>
        public void OnCreationComplete()
        {
            LowerOverlay.AddToClassList(TableVisualizerUss.CellOverlay);
            Insert(0, LowerOverlay);
        }

        #endregion

        #region Public Methods - Refresh

        /// <summary>
        /// Refreshes this cell control with current data and selection state.
        /// </summary>
        public void Refresh()
        {
            IsSelected = TableControl.CellSelector.IsCellSelected(Cell);
            HasFunction = !string.IsNullOrEmpty(TableControl.Metadata.GetFunction(Cell.Id)?.Trim());

            Cell.RefreshData();
            OnRefresh();
        }
        
        /// <summary>
        /// Refreshes this cell control with new cell data and table control.
        /// </summary>
        /// <param name="cell">The new cell data.</param>
        /// <param name="tableControl">The new table control.</param>
        public virtual void Refresh(Cell cell, TableControl tableControl)
        {
            TableControl = tableControl;
            Cell = cell;
            
            Refresh();
        }

        /// <summary>
        /// Recalculates the size of this cell based on its content.
        /// </summary>
        public void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSize(Cell, TableControl.Metadata);
            SetPreferredSize(size.x, size.y);
        }

        #endregion

        #region Protected Methods - Abstract

        /// <summary>
        /// Called when the cell needs to be refreshed.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract void OnRefresh();

        #endregion

        #region Protected Methods - Size Management

        /// <summary>
        /// Sets the preferred size for this cell.
        /// </summary>
        /// <param name="width">The preferred width.</param>
        /// <param name="height">The preferred height.</param>
        protected void SetPreferredSize(float width, float height)
        {
            TableControl.PreferredSize.AddCellSize(Cell, new Vector2(width, height));
        }

        #endregion

        #region Protected Methods - Value Management

        /// <summary>
        /// Sets the value of this cell with undo/redo support.
        /// </summary>
        /// <param name="value">The new value to set.</param>
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

        #endregion
    }
}