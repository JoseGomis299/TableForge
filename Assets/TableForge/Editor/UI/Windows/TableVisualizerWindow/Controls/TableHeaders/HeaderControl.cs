using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal abstract class HeaderControl : VisualElement
    {
        private readonly ContextualMenuManipulator _contextualMenuManipulator;
        private bool _isSelected;
        private bool _isSubSelected;
        
        public CellAnchor CellAnchor { get; protected set; }
        public TableControl TableControl { get; protected set; }


        public bool IsSubSelected
        {
            get => _isSubSelected;
            set
            {
                if (!value || _isSelected)
                    RemoveFromClassList(USSClasses.SubSelectedHeader);
                else
                    AddToClassList(USSClasses.SubSelectedHeader);

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
                    RemoveFromClassList(USSClasses.SelectedHeader);
                else
                    AddToClassList(USSClasses.SelectedHeader);

                _isSelected = value;
            }
        }
        public bool IsVisible { get; set; }
        public string Name => CellAnchor?.Name ?? string.Empty;
        public int Id => CellAnchor?.Id ?? 0;
        
        protected HeaderControl()
        {
            _contextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
        }

        public void Disable()
        {
            OnDisable();
        }
        
        protected virtual void OnEnable(CellAnchor cellAnchor, TableControl tableControl)
        {
            CellAnchor = cellAnchor;
            TableControl = tableControl;
            
            IsSelected = tableControl.CellSelector.IsAnchorSelected(cellAnchor);
            IsSubSelected = tableControl.CellSelector.IsAnchorSubSelected(cellAnchor);
            
            tableControl.CellSelector.OnSelectionChanged += OnSelectionChanged;
            
            if (tableControl.Parent == null)
            {
                this.AddManipulator(_contextualMenuManipulator);
            }
        }
        
        protected virtual void OnDisable()
        {
            TableControl.CellSelector.OnSelectionChanged -= OnSelectionChanged;
            this.RemoveManipulator(_contextualMenuManipulator);
        }

        private void OnSelectionChanged()
        {
            IsSelected = TableControl.CellSelector.IsAnchorSelected(CellAnchor);
            IsSubSelected = TableControl.CellSelector.IsAnchorSubSelected(CellAnchor);
        }

        protected virtual void SelectionChanged()
        {
            //Noop
        }

        protected abstract void BuildContextualMenu(ContextualMenuPopulateEvent obj);

        protected void ExpandCollapseBuilder(ContextualMenuPopulateEvent obj)
        {
            List<SubTableCell> selectedCells = TableControl.CellSelector.GetSelectedCells(TableControl.TableData).OfType<SubTableCell>().ToList();
            bool containsSubTable = selectedCells.Any();
            
            if (!containsSubTable) return;
            obj.menu.AppendAction("Expand All", (_) =>
            {
                SetExpanded(selectedCells, true);
            });

            obj.menu.AppendAction("Collapse All", (_) =>
            {
                SetExpanded(selectedCells, false);
            });
        }

        private void SetExpanded(IEnumerable<Cell> cells, bool value)
        {
            foreach (var cell in cells)
            {
                if(TableControl.Metadata.IsTableExpanded(cell.Id) == value) continue;
                        
                TableControl.Metadata.SetTableExpanded(cell.Id, value);
                TableControl.PreferredSize.AddCellSize(cell, SizeCalculator.CalculateSize(cell, TableControl.Metadata));
                TableControl.PreferredSize.StoreCellSizeInMetadata(cell);
            }

            TableControl.RebuildPage();
        }
    }
}