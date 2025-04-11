using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class ExpandableSubTableCellControl : SubTableCellControl
    {
        protected VisualElement ContentContainer;
        protected Foldout HeaderFoldout;
        protected bool HasSubTableInitialized;
        protected string FoldoutHeaderText ;
        
        public bool IsFoldoutOpen => HeaderFoldout.value;

        protected ExpandableSubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            FoldoutHeaderText = cell.Column.Name;

            CreateContainerStructure();
            InitializeFoldout();
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            
            FoldoutHeaderText = cell.Column.Name;
            HeaderFoldout.text = FoldoutHeaderText;
            
            bool isExpanded = TableControl.Metadata.IsTableExpanded(cell.Id);
            if(isExpanded && !HasSubTableInitialized)
            {
                InitializeSubTable();
            }
            HeaderFoldout.value = isExpanded;
            ContentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void OpenFoldout()
        {
            OnFoldoutToggled(ChangeEvent<bool>.GetPooled(false, true));
        }

        private void CreateContainerStructure()
        {
            HeaderFoldout = new Foldout { text = FoldoutHeaderText };
            HeaderFoldout.AddToClassList("table__subtable__foldout");
            
            ContentContainer = new VisualElement();
            ContentContainer.AddToClassList(USSClasses.SubTableContainer);

            ContentContainer.style.display = DisplayStyle.None;

            Add(HeaderFoldout);
            Add(ContentContainer);
        }

        private void InitializeFoldout()
        {
            HeaderFoldout.RegisterValueChangedCallback(OnFoldoutToggled);
            HeaderFoldout.value = false;
            HeaderFoldout.focusable = false;
        }
        
        private void InitializeSubTable()
        {
            BuildSubTable();
            HasSubTableInitialized = true;
            IsSelected = TableControl.CellSelector.SelectedCells.Contains(Cell);
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            ContentContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            bool wasSubTableInitialized = HasSubTableInitialized;
            TableControl.Metadata.SetTableExpanded(Cell.Id, evt.newValue);
            
            if (evt.newValue && !HasSubTableInitialized)
            {
                InitializeSubTable();
            }

            if (!evt.newValue || !wasSubTableInitialized)
            {
                RecalculateSizeWithCurrentValues();
                TableControl.Resizer.ResizeCell(this);
            }
            else
            {
                RecalculateSizeWithCurrentValues();
                SubTableControl.Resizer.ResizeAll(true);
                TableControl.Resizer.ResizeCell(this);
            }
            
            IsSelected = TableControl.CellSelector.SelectedCells.Contains(Cell);
        }
        
        protected abstract void BuildSubTable();

        protected override void RecalculateSizeWithCurrentValues()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);
            SetPreferredSize(size.x, size.y);
            TableControl.PreferredSize.StoreCellSizeInMetadata(Cell);
            
            if(TableControl.Parent is ExpandableSubTableCellControl expandableSubTableCellControl)
            {
                expandableSubTableCellControl.RecalculateSizeWithCurrentValues();
            }
        }
    }
}