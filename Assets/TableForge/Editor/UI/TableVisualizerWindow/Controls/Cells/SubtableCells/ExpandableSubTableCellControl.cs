using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class ExpandableSubTableCellControl : SubTableCellControl
    {
        protected VisualElement ContentContainer;
        protected VisualElement SubTableContentContainer;
        protected VisualElement SubTableToolbar;
        protected bool IsSubTableInitialized;
        
        private Foldout _headerFoldout;
        private string _foldoutHeaderText;
        
        private Button _collapseButton;
        
        public bool IsFoldoutOpen => _headerFoldout.value;

        protected ExpandableSubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _foldoutHeaderText = cell.Column.Name;

            CreateContainerStructure();
            InitializeFoldout();
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            base.Refresh(cell, tableControl);
            
            _foldoutHeaderText = cell.Column.Name;
            _headerFoldout.text = _foldoutHeaderText;
            
            bool isExpanded = TableControl.Metadata.IsTableExpanded(cell.Id);
            if(isExpanded && !IsSubTableInitialized)
            {
                InitializeSubTable();
            }

            ShowToolbar(isExpanded, true);
            ShowFoldout(!isExpanded);

            _headerFoldout.value = isExpanded;
            SubTableContentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public void OpenFoldout()
        {
            _headerFoldout.value = true;
        }
        
        public void CloseFoldout()
        {
            _headerFoldout.value = false;
        }

        private void CreateContainerStructure()
        {
            _headerFoldout = new Foldout { text = _foldoutHeaderText };
            _headerFoldout.AddToClassList(USSClasses.SubTableFoldout);
            
            _collapseButton = new Button();
            _collapseButton.AddToClassList(USSClasses.SubTableToolbarButton);
            var arrowElement = new VisualElement();
            arrowElement.AddToClassList(USSClasses.SubTableToolbarFoldout);
            _collapseButton.Add(arrowElement);
            
            ContentContainer = new VisualElement();
            ContentContainer.AddToClassList(USSClasses.SubTableCellContent);
            
            SubTableToolbar = new VisualElement();
            SubTableToolbar.AddToClassList(USSClasses.SubTableToolbar);
            
            SubTableContentContainer = new VisualElement();
            SubTableContentContainer.AddToClassList(USSClasses.SubTableContentContainer);

            SubTableContentContainer.style.display = DisplayStyle.None;
            SubTableToolbar.style.display = DisplayStyle.None;
            
            Add(_headerFoldout);
            Add(ContentContainer);
            ContentContainer.Add(SubTableToolbar);
            ContentContainer.Add(SubTableContentContainer);
            SubTableToolbar.Add(_collapseButton);
        }

        private void InitializeFoldout()
        {
            _headerFoldout.RegisterValueChangedCallback(OnFoldoutToggled);
            _headerFoldout.value = false;
            _headerFoldout.focusable = false;
            
            _collapseButton.focusable = false;
            _collapseButton.clicked += () =>
            {
                _headerFoldout.value = false;
            };
        }
        
        private void InitializeSubTable()
        {
            BuildSubTable();
            IsSubTableInitialized = true;
            IsSelected = TableControl.CellSelector.IsCellSelected(Cell);
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            SubTableContentContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            TableControl.Metadata.SetTableExpanded(Cell.Id, evt.newValue);
            
            if (evt.newValue && !IsSubTableInitialized)
            {
                InitializeSubTable();
            }

            RecalculateSizeWithCurrentValues();
            SubTableControl.Resizer.ResizeAll(true);
            TableControl.Resizer.ResizeCell(this);
            
            IsSelected = TableControl.CellSelector.IsCellSelected(Cell);
            
            ShowToolbar(evt.newValue, true);
            ShowFoldout(!evt.newValue);
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
        
        public void ShowToolbar(bool show, bool checkAncestors)
        {
            if(!_headerFoldout.value)
            {
                SubTableToolbar.style.display = DisplayStyle.None;
                return;
            }
            
            bool focused = !checkAncestors || Cell.GetAncestors(true).Any(x => TableControl.CellSelector.IsCellFocused(x));

            if (show && focused)
            {
                SubTableToolbar.style.display = DisplayStyle.Flex;
                SubTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
            }
            else
            {
                SubTableToolbar.style.display = DisplayStyle.None;
            }
        }
        
        private void ShowFoldout(bool show)
        {
            _headerFoldout.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}