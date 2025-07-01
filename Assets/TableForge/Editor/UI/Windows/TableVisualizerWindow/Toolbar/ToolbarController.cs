using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
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
        private Button _visibleFieldsButton;
        private ToolbarSearchField _filter;
        private TextField _functionTextField;
        
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
        
        public void FocusFunctionText()
        {
            if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null)
            {
                _functionTextField.value = string.Empty;
                _functionTextField.SetEnabled(false);
                return;
            }
            
            Cell focusedCell = _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell();
            string function = _selectedTab.GetFunction(focusedCell.Id);
            
            _functionTextField.SetEnabled(true);
            _functionTextField.value = function ?? string.Empty;
            _functionTextField.Focus();
            _functionTextField.cursorIndex = _functionTextField.value.Length;
        }
        
        public void CloseTab(TableMetadata table)
        {
            if (!_tabControls.TryGetValue(table, out var tab)) return;
            CloseTab(tab);
        }
        
        public void CloseTab(TabControl tab)
        {
            if (!_openTabs.Contains(tab.TableMetadata)) return;
            
            UndoRedoManager.StartCollection();
            CloseTabCommand command = new CloseTabCommand(OpenTabInternal, CloseTabInternal, tab);
            UndoRedoManager.Do(command);
            UndoRedoManager.EndCollection();
        }
        
        public void OpenTab(TableMetadata table)
        {
            if (_openTabs.Contains(table)) return;

            TabControl tab = new TabControl(this, table);
            if (UndoRedoManager.GetLastUndoCommand() == null)
            {
                OpenTabInternal(tab);
            }
            else
            {
                OpenTabCommand command = new OpenTabCommand(OpenTabInternal, CloseTabInternal, tab);
                UndoRedoManager.Do(command);
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

                if (_selectedTab != null && _tabControls.TryGetValue(_selectedTab, out var previousTab))
                {
                    previousTab.RemoveFromClassList(USSClasses.ToolbarTabSelected);
                    _selectedTab = null;
                }
                SelectTab(tableMetadata);
            };
            EditTableWindow.ShowWindow(viewModel);
        }
        
        public void SelectTab(TableMetadata tableMetadata)
        {
            if (tableMetadata == _selectedTab) return;

            //Do not store the first tab selection, as it is the default state
            if (_selectedTab == null)
            {
                ChangeTab(tableMetadata);
                return;
            }
            
            ChangeTabCommand command = new ChangeTabCommand(_selectedTab, tableMetadata, ChangeTab);
            UndoRedoManager.Do(command);
        }
        
        public void UpdateTableCache(TableMetadata tableMetadata, Table table)
        {
            if (tableMetadata == null) return;
            _cachedTables[tableMetadata] = table;
        }

        private void Initialize()
        {
            BindVisualElements();
            RegisterEvents();
            OpenStoredTabs();
            
            RefreshFunctionTextField();
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
            _filter = _toolbar.Q<ToolbarSearchField>("filter");
            _functionTextField = _toolbar.Q<TextField>("function-field");
            _visibleFieldsButton = _toolbar.Q<Button>("visible-fields-button");
            _visibleColumnsDropdown = new MultiSelectDropdown(new List<DropdownElement>(), _visibleFieldsButton);
        }

        private void RegisterEvents()
        {
            _addTabButton.RegisterCallback<ClickEvent>(e =>
            {
               AddTabWindow.ShowWindow(new AddTabViewModel(this));
            });

            _transposeTableButton.RegisterCallback<ClickEvent>(e =>
            {
                if (_selectedTab == null) return;
                
                _tableVisualizer.CurrentTable.Transpose();
                _tableVisualizer.CurrentTable.RebuildPage();
                _tableVisualizer.CurrentTable.HorizontalResizer.ResizeHeader(_tableVisualizer.CurrentTable.CornerContainer.CornerControl);
            });
            
            _visibleColumnsDropdown.onSelectionChanged += selectedItems =>
            {
                if (_selectedTab == null) return;

                foreach (var column in _tableVisualizer.CurrentTable.TableData.OrderedColumns)    
                {
                    _tableVisualizer.CurrentTable.Metadata.SetFieldVisible(column.Id, false);
                }
                
                foreach (var visibleField in selectedItems)
                {
                    _tableVisualizer.CurrentTable.Metadata.SetFieldVisible(visibleField.id, true);
                }
                
                _tableVisualizer.CurrentTable.RebuildPage(false);
            };
            
            _filter.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return) 
                {
                    _tableVisualizer.CurrentTable.Filterer.Filter(_filter.value); 
                }
            }, TrickleDown.TrickleDown);
            
            _filter.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null || evt.newValue.Trim().Length == 0)
                {
                    _tableVisualizer.CurrentTable.Filterer.Filter(string.Empty);
                }
            });
            
            _functionTextField.RegisterValueChangedCallback(evt =>
            {
                if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null) return;
                
                Cell focusedCell = _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell();
                _tableVisualizer.CurrentTable.FunctionExecutor.SetCellFunction(focusedCell, evt.newValue);
            });
            
            _functionTextField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null) return;
                
                _tableVisualizer.CurrentTable.FunctionExecutor.ExecuteAllFunctions();
                RefreshFunctionTextField();
            });
        }
        
        public void RefreshFunctionTextField()
        {
            if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null
                || _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell() is SubTableCell)
            {
                _functionTextField.RemoveFromChildrenClassList(USSClasses.ToolbarIncorrectFunctionField);
                _functionTextField.SetValueWithoutNotify("");
                _functionTextField.SetEnabled(false);
                return;
            }
            
            Cell focusedCell = _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell();
            string function = _selectedTab.GetFunction(focusedCell.Id);
            
            if(_tableVisualizer.CurrentTable.FunctionExecutor.IsCellFunctionCorrect(focusedCell.Id))
            {
                _functionTextField.RemoveFromChildrenClassList(USSClasses.ToolbarIncorrectFunctionField);
            }
            else
            {
                _functionTextField.AddToChildrenClassList(USSClasses.ToolbarIncorrectFunctionField);
            }
            
            _functionTextField.SetEnabled(true);
            _functionTextField.SetValueWithoutNotify(function ?? string.Empty);
        }

        private void OpenTabInternal(TabControl tab)
        {
            TableMetadata table = tab.TableMetadata;
            _tabContainer.Add(tab);
            _openTabs.Add(table);
            _orderedOpenTabs.Add(table);
            _tabControls.Add(table, tab);
            SessionCache.OpenTab(table);

            if(_selectedTab == null)
            {
                SelectTab(table);
            }
        }

        private void CloseTabInternal(TabControl tab)
        {
            _tabContainer.Remove(tab);
            _openTabs.Remove(tab.TableMetadata);
            _orderedOpenTabs.Remove(tab.TableMetadata);
            _tabControls.Remove(tab.TableMetadata);
            SessionCache.CloseTab(tab.TableMetadata);

            if (_selectedTab != tab.TableMetadata) return;
            SelectTab(_openTabs.Count > 0 ? _openTabs.First() : null);
        }

        private void ChangeTab(TableMetadata tableMetadata)
        {
            if (_selectedTab != null && _tabControls.TryGetValue(_selectedTab, out var previousTab))
            {
                previousTab.RemoveFromClassList(USSClasses.ToolbarTabSelected);
            }
            if (tableMetadata != null && _tabControls.TryGetValue(tableMetadata, out var newTab))
            {
                newTab.AddToClassList(USSClasses.ToolbarTabSelected);
            }
            
            if(_tableVisualizer.CurrentTable != null)
            {
                _tableVisualizer.CurrentTable.CellSelector.OnFocusedCellChanged -= RefreshFunctionTextField;
            }
            
            _selectedTab = tableMetadata;
            Table table = GetTable(tableMetadata);
            _tableVisualizer.SetTable(table);
            
            if(_tableVisualizer.CurrentTable != null)
            {
                _tableVisualizer.CurrentTable.CellSelector.OnFocusedCellChanged += RefreshFunctionTextField;
            }

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

        private Table GetTable(TableMetadata tableMetadata)
        {
            if(tableMetadata == null) return null;
            tableMetadata.UpdateRowsPosition();
            
            if (_cachedTables.TryGetValue(tableMetadata, out var table))
            {
                bool rowsMatch = true;
                var guids = tableMetadata.ItemGUIDs;
                
                if (guids.Count == table.Rows.Count && !tableMetadata.IsTypeBound)
                {
                    foreach (var row in table.Rows.Values)
                    {
                        if (!tableMetadata.HasGuid(row.SerializedObject.RootObjectGuid))
                        {
                            rowsMatch = false;
                            break;
                        }
                    }
                }
                else if (guids.Count != table.Rows.Count)
                {
                    rowsMatch = false;
                }
                
                if (!rowsMatch)
                {
                    table = TableMetadataManager.GetTable(tableMetadata);
                    _cachedTables[tableMetadata] = table;
                }
                
                return table;
            }

            table = TableMetadataManager.GetTable(tableMetadata);
            _cachedTables[tableMetadata] = table;
            return table;
        }
    }
}