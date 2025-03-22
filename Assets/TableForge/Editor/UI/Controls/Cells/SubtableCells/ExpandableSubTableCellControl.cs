using UnityEditor;
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
            InitializeSize();
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            
            FoldoutHeaderText = cell.Column.Name;
            HeaderFoldout.text = FoldoutHeaderText;
            
            bool isExpanded = TableControl.Metadata.CellMetadata.TryGetValue(Cell.Id, out CellMetadata metadata) && metadata.isExpanded;
            if(isExpanded && !HasSubTableInitialized)
            {
                InitializeSubTable();
                HasSubTableInitialized = true;
            }
            HeaderFoldout.value = isExpanded;
            ContentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

            InitializeSize();            
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
            
            IsSelected = false;
        }

        private void InitializeFoldout()
        {
            HeaderFoldout.RegisterValueChangedCallback(OnFoldoutToggled);
            HeaderFoldout.value = false;
        }

        protected virtual void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            ContentContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            bool wasSubTableInitialized = HasSubTableInitialized;
            TableMetadataManager.SetCellExpandedState(TableControl.Metadata, Cell.Id, evt.newValue);
            
            if (evt.newValue && !HasSubTableInitialized)
            {
                InitializeSubTable();
                HasSubTableInitialized = true;
            }

            if (!evt.newValue || !wasSubTableInitialized)
            {
                InitializeSize();
                TableControl.HorizontalResizer.ResizeCell(this);
                TableControl.VerticalResizer.ResizeCell(this);
            }
            else
            {
                RecalculateSize();
                TableControl.HorizontalResizer.ResizeCell(this);
                TableControl.VerticalResizer.ResizeCell(this);
            }
        }

        protected abstract void InitializeSubTable();

        protected override void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);
            SetDesiredSize(size.x, size.y + UiConstants.FoldoutHeight);
        }
    }
}