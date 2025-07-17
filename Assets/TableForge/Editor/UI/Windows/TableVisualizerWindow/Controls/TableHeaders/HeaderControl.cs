using TableForge.Editor.UI.UssClasses;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Abstract base class for table header controls that provides common functionality
    /// for row and column headers including selection state management.
    /// </summary>
    internal abstract class HeaderControl : VisualElement
    {
        #region Fields

        protected static readonly SubTableHeaderContextMenuBuilder SubTableHeaderContextMenuBuilder = new();
        
        private readonly ContextualMenuManipulator _contextualMenuManipulator;
        private bool _isSelected;
        private bool _isSubSelected;

        #endregion

        #region Public Properties

        public CellAnchor CellAnchor { get; protected set; }
        public TableControl TableControl { get; protected set; }

        public bool IsSubSelected
        {
            get => _isSubSelected;
            set
            {
                if (!value || _isSelected)
                    RemoveFromClassList(TableVisualizerUss.SubSelectedHeader);
                else
                    AddToClassList(TableVisualizerUss.SubSelectedHeader);

                _isSubSelected = value;
                SelectionChanged();
            }
        }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!value)
                    RemoveFromClassList(TableVisualizerUss.SelectedHeader);
                else
                    AddToClassList(TableVisualizerUss.SelectedHeader);

                _isSelected = value;
            }
        }
        
        public bool IsVisible { get; set; }
        public string Name => CellAnchor?.Name ?? string.Empty;
        public int Id => CellAnchor?.Id ?? 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the HeaderControl class.
        /// </summary>
        protected HeaderControl()
        {
            _contextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Disables this header control and cleans up resources.
        /// </summary>
        public void Disable()
        {
            OnDisable();
        }

        #endregion

        #region Protected Methods - Lifecycle

        /// <summary>
        /// Enables this header control with the specified cell anchor and table control.
        /// </summary>
        /// <param name="cellAnchor">The cell anchor to associate with this header.</param>
        /// <param name="tableControl">The table control that owns this header.</param>
        protected virtual void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            CellAnchor = cellAnchor;
            TableControl = tableControl;
            
            IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
            IsSubSelected = tableControl.CellSelector.IsAnchorSubSelected(cellAnchor);
            
            tableControl.CellSelector.OnSelectionChanged += OnSelectionChanged;
            
          
            this.AddManipulator(_contextualMenuManipulator);
        }
        
        /// <summary>
        /// Disables this header control and cleans up event subscriptions.
        /// </summary>
        protected virtual void OnDisable()
        {
            TableControl.CellSelector.OnSelectionChanged -= OnSelectionChanged;
            this.RemoveManipulator(_contextualMenuManipulator);
        }

        /// <summary>
        /// Called when the selection state of this header changes.
        /// Override in derived classes to handle selection changes.
        /// </summary>
        protected virtual void SelectionChanged()
        {
            // Default implementation does nothing
        }

        #endregion

        #region Protected Methods - Context Menu

        /// <summary>
        /// Gets the context menu builder for this header type.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <returns>The context menu builder for this header type.</returns>
        protected abstract IHeaderContextMenuBuilder GetContextMenuBuilder();

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles selection change events from the cell selector.
        /// </summary>
        private void OnSelectionChanged()
        {
            IsSelected = TableControl.CellSelector.IsAnchorSelected(CellAnchor);
            IsSubSelected = TableControl.CellSelector.IsAnchorSubSelected(CellAnchor);
        }

        /// <summary>
        /// Builds the contextual menu for this header using the appropriate context menu builder.
        /// </summary>
        /// <param name="evt">The contextual menu populate event.</param>
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var contextMenuBuilder = TableControl.Parent != null ? SubTableHeaderContextMenuBuilder : GetContextMenuBuilder();
            contextMenuBuilder?.BuildContextMenu(this, evt);
        }

        #endregion
    }
}