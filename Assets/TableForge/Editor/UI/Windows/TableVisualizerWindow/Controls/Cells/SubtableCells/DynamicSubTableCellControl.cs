namespace TableForge.Editor.UI
{
    internal abstract class DynamicSubTableCellControl : ExpandableSubTableCellControl
    {
        private readonly IRowAdditionStrategy _rowAdditionStrategy;
        private readonly IRowDeletionStrategy _rowDeletionStrategy;
        
        private AddRowControl _addRowButton;
        private DeleteRowControl _deleteRowButton;
        
        protected DynamicSubTableCellControl(SubTableCell cell, TableControl tableControl, IRowAdditionStrategy rowAdditionStrategy, IRowDeletionStrategy rowDeletionStrategy) : base(cell, tableControl)
        {
            _rowAdditionStrategy = rowAdditionStrategy;
            _rowDeletionStrategy = rowDeletionStrategy;
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            if(SubTableControl != null && SubTableControl.RowHeaders.Count != SubTableControl.TableData.Rows.Count)
            {
                SubTableControl.RebuildPage();
            }
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            
            if(SubTableControl?.TableData == null) return;
            subTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
        }

        protected void ShowAddRowButton(bool show)
        {
            if(show && _addRowButton == null)
            {
                _addRowButton = new AddRowControl(SubTableControl, _rowAdditionStrategy);
                _addRowButton.OnRowAdded += OnRowAdded;
                subTableToolbar.Add(_addRowButton);
            }
            else if(!show && _addRowButton != null)
            {
                _addRowButton?.RemoveFromHierarchy();
                _addRowButton = null;
            }
        }
        
        protected void ShowDeleteRowButton(bool show)
        {
            if(show && _deleteRowButton == null)
            {
                _deleteRowButton = new DeleteRowControl(SubTableControl, _rowDeletionStrategy);
                _deleteRowButton.OnRowDeleted += OnRowDeleted;
                subTableToolbar.Add(_deleteRowButton);
            }
            else if(!show && _deleteRowButton != null)
            {
                _deleteRowButton?.RemoveFromHierarchy();
                _deleteRowButton = null;
            }
        }
        
        public virtual void OnRowAdded()
        {
            RecalculateSizeWithCurrentValues();
            TableControl.VerticalResizer.ResizeCell(this);
            
            if(SubTableControl.TableData.Rows.Count > 0)
            {
                ShowDeleteRowButton(true);
                subTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
            }
        }
        
        public void OnRowDeleted()
        {
            if(SubTableControl.TableData.Rows.Count == 0)
            {
                ShowDeleteRowButton(false);
                ShowAddRowButton(true);
                subTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
            }
            
            RecalculateSizeWithCurrentValues();
            TableControl.VerticalResizer.ResizeCell(this);
        }
        
    }
}