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
            
            HeaderFoldout.value = false;
            ContentContainer.style.display = DisplayStyle.None;

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
            
            if (evt.newValue && !HasSubTableInitialized)
            {
                InitializeSubTable();
                HasSubTableInitialized = true;
            }

            if (!evt.newValue)
            {
                InitializeSize();
                TableControl.HorizontalResizer.ResizeCell(this);
                TableControl.VerticalResizer.ResizeCell(this);
            }
            else
            {
                RecalculateSize();
            }
        }

        protected abstract void InitializeSubTable();

        protected override void RecalculateSize()
        {
            Vector2 size = SizeCalculator.CalculateSizeWithCurrentCellSizes(SubTableControl);
            SetDesiredSize(size.x, size.y + UiConstants.FoldoutHeight);

            TableControl.HorizontalResizer.ResizeCell(this);
            TableControl.VerticalResizer.ResizeCell(this);
        }
    }
}