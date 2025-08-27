using TableForge.Editor.UI.UssClasses;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Control for managing row headers in the table visualizer.
    /// Handles row selection, renaming, and contextual menu operations.
    /// </summary>
    internal class RowHeaderControl : HeaderControl
    {
        #region Private Fields

        private static readonly ObjectPool<RowHeaderControl> _pool = new(() => new RowHeaderControl());
        private static readonly ObjectPool<RowControl> _rowControlPool = new(() => new RowControl());
        private static readonly RowHeaderContextMenuBuilder _contextMenuBuilder = new();
        private static readonly ColumnHeaderContextMenuBuilder _transposedContextMenuBuilder = new();
        
        private bool _isChangingName;
        private readonly Label _headerLabel;
        private readonly TextField _textField;

        #endregion

        #region Public Properties

        public RowControl RowControl { get; set; }
        public bool IsChangingName => _isChangingName;

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets a pooled instance of RowHeaderControl and initializes it.
        /// </summary>
        /// <param name="cellAnchor">The cell anchor for this header.</param>
        /// <param name="tableControl">The table control that owns this header.</param>
        /// <returns>A configured RowHeaderControl instance.</returns>
        public static RowHeaderControl GetPooled(CellAnchor cellAnchor, TableControl tableControl)
        {
            var control = _pool.Get();
            control.OnEnable(cellAnchor, tableControl);
            return control;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RowHeaderControl class.
        /// </summary>
        private RowHeaderControl()
        {
            AddToClassList(TableVisualizerUss.TableHeaderCellVertical);
            
            _headerLabel = new Label();
            _headerLabel.AddToClassList(TableVisualizerUss.TableHeaderText);
            _textField = new TextField();
            
            _textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                _isChangingName = false;
                TryChangeName();
            });
            
            Add(_headerLabel);
        }

        #endregion

        #region Protected Methods - Lifecycle

        /// <summary>
        /// Enables this row header control with the specified cell anchor and table control.
        /// </summary>
        /// <param name="cellAnchor">The cell anchor to associate with this header.</param>
        /// <param name="tableControl">The table control that owns this header.</param>
        protected override void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            base.OnEnable(cellAnchor, tableControl);
            RowControl = _rowControlPool.Get();
            if (!tableControl.Filterer.IsVisible(CellAnchor.GetRootAnchor().Id))
            {
                style.display = DisplayStyle.None;
                if(RowControl != null)
                    RowControl.style.display = DisplayStyle.None;
                return;
            }
            
            style.display = DisplayStyle.Flex;
            RowControl.style.display = DisplayStyle.Flex;
            
            if(tableControl.Parent != null)
                AddToClassList(TableVisualizerUss.SubTableHeaderCellVertical);
            else
                RemoveFromClassList(TableVisualizerUss.SubTableHeaderCellVertical);
            
            var title = NameResolver.ResolveHeaderStyledName(cellAnchor, tableControl.TableAttributes.rowHeaderVisibility);
            _headerLabel.text = title;
            if(tableControl.Parent != null)
                _headerLabel.AddToClassList(TableVisualizerUss.SubTableHeaderText);
            else
                _headerLabel.RemoveFromClassList(TableVisualizerUss.SubTableHeaderText);
            
            _textField.value = cellAnchor.Name;
            
            TableControl.VerticalResizer.HandleResize(this);
            TableControl.HeaderSwapper.HandleSwapping(this);
            
            RowControl.Initialize(cellAnchor, tableControl);
        }

        /// <summary>
        /// Disables this row header control and cleans up resources.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            TableControl.VerticalResizer.Dispose(this);
            TableControl.HeaderSwapper.Dispose(this);
            RowControl.ClearRow();
            RowControl.style.height = 0;
            style.height = 0;
            
            _rowControlPool.Release(RowControl);
            _pool.Release(this);
        }

        /// <summary>
        /// Handles selection changes for this row header.
        /// </summary>
        protected override void SelectionChanged()
        {
            if(!IsSelected && _isChangingName)
            {
                _isChangingName = false;
                TryChangeName();
            }
        }

        #endregion

        #region Protected Methods - Context Menu

        /// <summary>
        /// Gets the context menu builder for row headers.
        /// </summary>
        /// <returns>The row header context menu builder.</returns>
        protected override IHeaderContextMenuBuilder GetContextMenuBuilder()
        {
            return TableControl.Transposed? _transposedContextMenuBuilder : _contextMenuBuilder;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes this row header and its associated row control.
        /// </summary>
        public void Refresh()
        {
            if(!TableControl.Filterer.IsVisible(CellAnchor.GetRootAnchor().Id)) return;
            
            RowControl.Refresh();
            RefreshName();
        }

        /// <summary>
        /// Refreshes the display name of this row header.
        /// </summary>
        public void RefreshName()
        {
            _headerLabel.text = NameResolver.ResolveHeaderStyledName(CellAnchor, TableControl.TableAttributes.rowHeaderVisibility);
        }

        /// <summary>
        /// Starts the name editing process for this row header.
        /// </summary>
        public void StartNameEditing()
        {
            _isChangingName = true;
            
            _textField.value = CellAnchor.Name;
            _textField.AddToClassList(TableVisualizerUss.TableHeaderText);
            _textField.RegisterCallback<KeyDownEvent>((keyEvt) =>
            {
                if (keyEvt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                {
                     TryChangeName();
                    _isChangingName = false;
                }
                else if (keyEvt.keyCode == KeyCode.Escape)
                {
                    HideTextField();
                    _isChangingName = false;
                }
            });

            Remove(_headerLabel);
            Add(_textField);
            _textField.Focus();
            _textField.SelectAll();
        }

        /// <summary>
        /// Removes this row from the table.
        /// </summary>
        public void RemoveThisRow()
        {
            TableControl.RemoveRow(CellAnchor.Id);
            TableControl.RebuildPage();
        }
        
        /// <summary>
        /// Removes all selected rows from the table.
        /// </summary>
        public void RemoveSelectedRows()
        {
            UndoRedoManager.StartCollection();
            var selectedRows = TableControl.CellSelector.GetSelectedRows(TableControl.TableData);
            selectedRows.Sort((a, b) => b.Position.CompareTo(a.Position));

            foreach (var selected in selectedRows)
            {
                if (selected.Table != TableControl.TableData) continue;
                TableControl.RemoveRow(selected.Id);
            }
            UndoRedoManager.EndCollection();

            TableControl.RebuildPage();
        }

        #endregion

        #region Private Methods - Name Editing

        /// <summary>
        /// Attempts to change the name of the associated asset.
        /// </summary>
        private void TryChangeName()
        {
            string path = AssetDatabase.GetAssetPath(((Row)CellAnchor).SerializedObject.RootObject);
            AssetUtils.RenameAsset(path, _textField.value.Trim());
            HideTextField();
        }

        /// <summary>
        /// Hides the text field and restores the label display.
        /// </summary>
        private void HideTextField()
        {
            RefreshName();
            Remove(_textField);
            Add(_headerLabel);
            
            //Recover focus on the window in case we lost it
            schedule.Execute(() =>
            {
                TableControl.Root.Focus();
            }).ExecuteLater(0);
        }

        #endregion
    }
}