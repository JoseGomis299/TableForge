using System.Linq;
using TableForge.Editor.UI.UssClasses;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal abstract class ExpandableSubTableCellControl : SubTableCellControl
    {
        private VisualElement _foldoutContentContainer;
        protected VisualElement subTableContentContainer;
        protected VisualElement subTableToolbar;
        
        private Foldout _headerFoldout;
        private string _foldoutHeaderText;
        
        private Button _collapseButton;
        
        public bool IsFoldoutOpen => _headerFoldout.value;
        protected bool IsSubTableInitialized => SubTableControl is { TableData: not null };

        protected ExpandableSubTableCellControl(SubTableCell cell, TableControl tableControl) : base(cell, tableControl)
        {
            _foldoutHeaderText = cell.column.Name;

            CreateContainerStructure();
            InitializeFoldout();
        }

        public override void Refresh(Cell cell, TableControl tableControl)
        {
            bool isExpanded = TableControl.Metadata.IsTableExpanded(cell.Id);
            _headerFoldout.value = isExpanded;
            subTableContentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            
            base.Refresh(cell, tableControl);
            
            _foldoutHeaderText = cell.column.Name;
            _headerFoldout.text = _foldoutHeaderText;
            
            if(isExpanded && !IsSubTableInitialized)
            {
                InitializeSubTable();
            }
            
            if(isExpanded && SubTableControl.TableData != ((SubTableCell)Cell).SubTable)
            {
                RecalculateSizeWithCurrentValues();
                SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
            }

            ShowToolbar(isExpanded, true);
            ShowFoldout(!isExpanded);
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
            _headerFoldout.AddToClassList(TableVisualizerUss.SubTableFoldout);
            
            _collapseButton = new Button();
            _collapseButton.AddToClassList(TableVisualizerUss.SubTableToolbarButton);
            var arrowElement = new VisualElement();
            arrowElement.AddToClassList(TableVisualizerUss.SubTableToolbarFoldout);
            _collapseButton.Add(arrowElement);
            
            _foldoutContentContainer = new VisualElement();
            _foldoutContentContainer.AddToClassList(TableVisualizerUss.SubTableCellContent);
            
            subTableToolbar = new VisualElement();
            subTableToolbar.AddToClassList(TableVisualizerUss.SubTableToolbar);
            
            subTableContentContainer = new VisualElement();
            subTableContentContainer.AddToClassList(TableVisualizerUss.SubTableContentContainer);

            subTableContentContainer.style.display = DisplayStyle.None;
            subTableToolbar.style.display = DisplayStyle.None;
            
            Add(_headerFoldout);
            Add(_foldoutContentContainer);
            _foldoutContentContainer.Add(subTableToolbar);
            _foldoutContentContainer.Add(subTableContentContainer);
            subTableToolbar.Add(_collapseButton);
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
            IsSelected = TableControl.CellSelector.IsCellSelected(Cell);
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            subTableContentContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            TableControl.Metadata.SetTableExpanded(Cell.Id, evt.newValue);
            
            if (evt.newValue && !IsSubTableInitialized)
            {
                InitializeSubTable();
            }

            if(SubTableControl.TableData != ((SubTableCell)Cell).SubTable)
            {
                SubTableControl.SetTable(((SubTableCell)Cell).SubTable);
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
        
        public void ShowToolbar(bool show, bool checkDescendants)
        {
            if(!_headerFoldout.value)
            {
                subTableToolbar.style.display = DisplayStyle.None;
                return;
            }
            
            bool focused = !checkDescendants || Cell.GetDescendants(includeSelf:true).Any(x => TableControl.CellSelector.IsCellFocused(x));

            if (show && focused && subTableToolbar.style.display != DisplayStyle.Flex)
            {
                subTableToolbar.style.display = DisplayStyle.Flex;
                subTableToolbar.style.height = SizeCalculator.CalculateToolbarSize(SubTableControl.TableData).y;
            }
            else if ((!show || !focused) && subTableToolbar.style.display != DisplayStyle.None)
            {
                subTableToolbar.style.display = DisplayStyle.None;
            }
        }
        
        private void ShowFoldout(bool show)
        {
            _headerFoldout.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}