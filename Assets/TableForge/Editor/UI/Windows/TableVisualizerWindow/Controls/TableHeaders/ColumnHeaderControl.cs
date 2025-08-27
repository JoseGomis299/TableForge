using TableForge.Editor.UI.UssClasses;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Control for managing column headers in the table visualizer.
    /// Handles column selection and contextual menu operations.
    /// </summary>
    internal class ColumnHeaderControl : HeaderControl
    {
        #region Private Fields

        private static readonly ObjectPool<ColumnHeaderControl> _pool = new(() => new ColumnHeaderControl());
        private static readonly ColumnHeaderContextMenuBuilder _contextMenuBuilder = new();
        private readonly Label _headerLabel;

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets a pooled instance of ColumnHeaderControl and initializes it.
        /// </summary>
        /// <param name="cellAnchor">The cell anchor for this header.</param>
        /// <param name="tableControl">The table control that owns this header.</param>
        /// <returns>A configured ColumnHeaderControl instance.</returns>
        public static ColumnHeaderControl GetPooled(CellAnchor cellAnchor, TableControl tableControl)
        {
            var control = _pool.Get();
            control.OnEnable(cellAnchor, tableControl);
            return control;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ColumnHeaderControl class.
        /// </summary>
        private ColumnHeaderControl()
        {
            AddToClassList(TableVisualizerUss.TableHeaderCellHorizontal);
            _headerLabel = new Label();
        }

        #endregion

        #region Protected Methods - Lifecycle

        /// <summary>
        /// Enables this column header control with the specified cell anchor and table control.
        /// </summary>
        /// <param name="cellAnchor">The cell anchor to associate with this header.</param>
        /// <param name="tableControl">The table control that owns this header.</param>
        protected override void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            base.OnEnable(cellAnchor, tableControl);
            if (!tableControl.Filterer.IsVisible(CellAnchor.GetRootAnchor().Id))
            {
                style.display = DisplayStyle.None;
                return;
            }
            
            style.display = DisplayStyle.Flex;
            string title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.columnHeaderVisibility);
            _headerLabel.text = title;
            _headerLabel.AddToClassList(TableVisualizerUss.TableHeaderText);
            if(tableControl.Parent != null)
                _headerLabel.AddToClassList(TableVisualizerUss.SubTableHeaderText);
            else 
                _headerLabel.RemoveFromClassList(TableVisualizerUss.SubTableHeaderText);
            Add(_headerLabel);
            
            TableControl.HorizontalResizer.HandleResize(this);
        }

        /// <summary>
        /// Disables this column header control and cleans up resources.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            style.width = 0;
            TableControl.HorizontalResizer.Dispose(this);
            _pool.Release(this);
        }

        #endregion

        #region Protected Methods - Context Menu

        /// <summary>
        /// Gets the context menu builder for column headers.
        /// </summary>
        /// <returns>The column header context menu builder.</returns>
        protected override IHeaderContextMenuBuilder GetContextMenuBuilder()
        {
            return TableControl.Transposed? SubTableHeaderContextMenuBuilder : _contextMenuBuilder;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the display name of this column header.
        /// </summary>
        public void RefreshName()
        {
            _headerLabel.text = NameResolver.ResolveHeaderStyledName(CellAnchor, TableControl.TableAttributes.columnHeaderVisibility);
        }

        #endregion
    }
}