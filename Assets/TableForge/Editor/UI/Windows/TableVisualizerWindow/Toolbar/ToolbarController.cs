using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class ToolbarController
    {
        public event Action<TableMetadata> OnEditionComplete;
        
        private readonly VisualElement _toolbar;
        private readonly TableVisualizer _tableVisualizer;

        private Button _addTabButton;
        private Button _transposeTableButton;
        private VisualElement _tabContainer;
        private MultiSelectDropdown _visibleColumnsDropdown;
        
        private TableMetadata _selectedTab;
        private readonly Dictionary<TableMetadata, TabControl> _tabControls = new();
        private readonly Dictionary<TableMetadata, Table> _cachedTables = new();
        private readonly List<TableMetadata> _orderedOpenTabs = new();
        private readonly HashSet<TableMetadata> _openTabs = new();
        
        public IReadOnlyList<TableMetadata> OpenTabs => _orderedOpenTabs;
        public TableMetadata SelectedTab => _selectedTab;

        public ToolbarController(VisualElement toolbar, TableVisualizer tableVisualizer)
        {
            _toolbar = toolbar;
            _tableVisualizer = tableVisualizer;
            
            Initialize();
        }

        private void Initialize()
        {
            CreateVisualElements();
            BindVisualElements();
            RegisterEvents();
            OpenStoredTabs();
        }

        private void CreateVisualElements()
        {
            VisualElement toolsParent = _toolbar.Q<VisualElement>("table-tools");
            _visibleColumnsDropdown = new MultiSelectDropdown(new List<DropdownElement>(), "Visible columns: ");
            toolsParent.Add(_visibleColumnsDropdown);
        }

        private void OpenStoredTabs()
        {
            foreach (var table in  SessionCache.GetOpenTabs())
            {
                OpenTab(table);
            }
        }

        private void BindVisualElements()
        {
            _addTabButton = _toolbar.Q<Button>("add-tab-button");
            _transposeTableButton = _toolbar.Q<Button>("transpose-button");
            _tabContainer = _toolbar.Q<VisualElement>("tab-container");
        }

        private void RegisterEvents()
        {
            _addTabButton.RegisterCallback<ClickEvent>(e =>
            {
               AddTabWindow.ShowWindow(new AddTabViewModel(this));
            });

            _transposeTableButton.RegisterCallback<ClickEvent>(e =>
            {
                _tableVisualizer.CurrentTable.Transpose();
                _tableVisualizer.CurrentTable.RebuildPage();
            });
            
            _visibleColumnsDropdown.OnSelectionChanged += selectedItems =>
            {
                if (_selectedTab == null) return;

                foreach (var column in _tableVisualizer.CurrentTable.TableData.OrderedColumns)    
                {
                    _tableVisualizer.CurrentTable.Metadata.SetFieldVisible(column.Id, false);
                }
                
                foreach (var visibleField in selectedItems)
                {
                    _tableVisualizer.CurrentTable.Metadata.SetFieldVisible(visibleField.Id, true);
                }
                
                _tableVisualizer.CurrentTable.RebuildPage();
            };
        }

        public void OpenTab(TableMetadata table)
        {
            if (_openTabs.Contains(table)) return;
            
            TabControl tabControl = new TabControl(this, table);
            _tabContainer.Add(tabControl);
            _openTabs.Add(table);
            _orderedOpenTabs.Add(table);
            _tabControls.Add(table, tabControl);
            SessionCache.OpenTab(table);

            if(_selectedTab == null)
            {
                SelectTab(table);
            }
        }
        
        public void CloseTab(TableMetadata table)
        {
            if (!_tabControls.TryGetValue(table, out var tab)) return;
            CloseTab(tab);
        }
        
        public void CloseTab(TabControl tab)
        {
            if (!_openTabs.Contains(tab.TableMetadata)) return;
            
            _tabContainer.Remove(tab);
            _openTabs.Remove(tab.TableMetadata);
            _orderedOpenTabs.Remove(tab.TableMetadata);
            _cachedTables.Remove(tab.TableMetadata);
            _tabControls.Remove(tab.TableMetadata);
            SessionCache.CloseTab(tab.TableMetadata);

            if (_selectedTab != tab.TableMetadata) return;
            SelectTab(_openTabs.Count > 0 ? _openTabs.First() : null);
        }

        public void SelectTab(TableMetadata tableMetadata)
        {
            if (tableMetadata == _selectedTab) return;
            
            if (_selectedTab != null && _tabControls.TryGetValue(_selectedTab, out var previousTab))
            {
                previousTab.RemoveFromClassList(USSClasses.ToolbarTabSelected);
            }
            if (tableMetadata != null && _tabControls.TryGetValue(tableMetadata, out var newTab))
            {
                newTab.AddToClassList(USSClasses.ToolbarTabSelected);
            }
            
            _selectedTab = tableMetadata;
            Table table = GetTable(tableMetadata);
            _tableVisualizer.SetTable(table);

            if (tableMetadata != null)
            {
                List<DropdownElement> selectedItems = table.OrderedColumns.Where(c => tableMetadata.IsFieldVisible(c.Id)).Select(c => new DropdownElement(c.Id, c.Name)).ToList();
                List<DropdownElement> allItems = table.OrderedColumns.Select(c => new DropdownElement(c.Id, c.Name)).ToList();
                _visibleColumnsDropdown.SetItems(allItems, selectedItems);
            }
            else
            {
                _visibleColumnsDropdown.SetItems(new List<DropdownElement>(), new List<DropdownElement>());
            }
        }

        public void EditTab(TableMetadata tableMetadata)
        {
            _cachedTables.Remove(tableMetadata);
            EditTableViewModel viewModel = new EditTableViewModel(tableMetadata);
            viewModel.OnTableUpdated += table =>
            {
                OnEditionComplete?.Invoke(table);
                
                Table newTable = TableMetadataManager.GetTable(table);
                _cachedTables[tableMetadata] = newTable;
                _selectedTab = null;
                SelectTab(tableMetadata);
            };
            EditTableWindow.ShowWindow(viewModel);
        }
        
        public void UpdateTableCache(TableMetadata tableMetadata, Table table)
        {
            if (tableMetadata == null) return;
            _cachedTables[tableMetadata] = table;
        }

        private Table GetTable(TableMetadata tableMetadata)
        {
            if(tableMetadata == null) return null;
            
            if (_cachedTables.TryGetValue(tableMetadata, out var table))
            {
                return table;
            }

            table = TableMetadataManager.GetTable(tableMetadata);
            _cachedTables[tableMetadata] = table;
            return table;
        }
    }
}