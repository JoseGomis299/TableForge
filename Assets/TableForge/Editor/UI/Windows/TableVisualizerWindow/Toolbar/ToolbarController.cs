using System;
using System.Collections.Generic;
using System.Linq;
using TableForge.Editor.UI.CustomControls;
using TableForge.Editor.UI.UssClasses;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class ToolbarController
    {
        public event Action<TableMetadata> OnEditionComplete;
        
        //References
        private readonly VisualElement _toolbar;
        private readonly TableVisualizer _tableVisualizer;

        //UI elements
        private Button _addTabButton;
        private Button _transposeTableButton;
        private Button _rebuildButton;
        private Button _adjustSizeButton;
        private VisualElement _tabContainer;
        private MultiSelectDropdownButton _visibleColumnsDropdown;
        private ToolbarSearchField _filter;
        private TextField _functionTextField;
        private Label _currentCellLabel;
        private FloatField _pollingIntervalField;

        //ButtonOverriders
        private ToggleButton _columnsLetterToggle;
        private ToggleButton _rowsNumberToggle;
        private ToggleButton _removeFormulaOnCellChangeToggle;
        private ToggleButton _pollingToggle;
        
        //State
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
            if(focusedCell is SubTableCell) return;
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
            EditTableViewModel viewModel = new EditTableViewModel(tableMetadata);
            viewModel.OnTableUpdated += table =>
            {
                OnEditionComplete?.Invoke(table);
                
                Table newTable = TableMetadataManager.GetTable(table);
                _cachedTables[tableMetadata] = newTable;

                if (_selectedTab != null && _tabControls.TryGetValue(_selectedTab, out var previousTab))
                {
                    previousTab.RemoveFromClassList(TableVisualizerUss.ToolbarTabSelected);
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
            RecoverSettingValues();
            RegisterEvents();
            OpenStoredTabs();
            
            OnFocusedCellChanged();
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
            _rebuildButton = _toolbar.Q<Button>("rebuild-button");
            _adjustSizeButton = _toolbar.Q<Button>("adjust-size-button");
            _tabContainer = _toolbar.Q<VisualElement>("tab-container");
            _filter = _toolbar.Q<ToolbarSearchField>("filter");
            _functionTextField = _toolbar.Q<TextField>("function-field");
            _visibleColumnsDropdown = new MultiSelectDropdownButton(_toolbar.Q<Button>("visible-fields-button"));
            _currentCellLabel = _toolbar.Q<Label>("current-cell-label");
            _columnsLetterToggle = new ToggleButton(_toolbar.Q<Button>("column-letter-toggle"));
            _rowsNumberToggle = new ToggleButton(_toolbar.Q<Button>("row-number-toggle"));
            _removeFormulaOnCellChangeToggle = new ToggleButton(_toolbar.Q<Button>("remove-formula-on-cell-change-toggle"));
            _pollingToggle = new ToggleButton(_toolbar.Q<Button>("polling-toggle"));
            _pollingIntervalField = _toolbar.Q<FloatField>("polling-interval-field");
        }
        
        private void RecoverSettingValues()
        {
            TableSettingsData settings = TableSettings.GetSettings();
            _columnsLetterToggle.SetState(settings.columnHeaderVisibility == TableHeaderVisibility.ShowHeaderLetterAndName);
            _rowsNumberToggle.SetState(settings.rowHeaderVisibility == TableHeaderVisibility.ShowHeaderNumberAndName);
            _removeFormulaOnCellChangeToggle.SetState(settings.removeFormulaOnCellValueChange);
            _pollingToggle.SetState(settings.enablePolling);
            _pollingIntervalField.SetValueWithoutNotify(settings.pollingInterval);
            
            _pollingIntervalField.style.display = settings.enablePolling ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RegisterEvents()
        {
            _addTabButton.RegisterCallback<ClickEvent>(_ =>
            {
               AddTabWindow.ShowWindow(new AddTabViewModel(this));
            });

            _transposeTableButton.RegisterCallback<ClickEvent>(_ =>
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
            
            _functionTextField.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null) return;
                
                _tableVisualizer.CurrentTable.FunctionExecutor.ExecuteAllFunctions();
                RefreshFunctionTextField();
            });
            
            _rebuildButton.clicked += () =>
            {
                if (_selectedTab == null) return;
                _tableVisualizer.CurrentTable.RebuildPage();
            };
            
            _columnsLetterToggle.OnValueChanged += (value =>
            {
                TableSettings.GetSettings().columnHeaderVisibility = value 
                    ? TableHeaderVisibility.ShowHeaderLetterAndName
                    : TableHeaderVisibility.ShowHeaderName;
                
                if (_selectedTab == null) return;
                
                _tableVisualizer.CurrentTable.TableAttributes.columnHeaderVisibility = TableSettings.GetSettings().columnHeaderVisibility;
                _tableVisualizer.CurrentTable.RefreshHeaderNames();
            });
            
            _rowsNumberToggle.OnValueChanged += (value =>
            {
                TableSettings.GetSettings().rowHeaderVisibility = value 
                    ? TableHeaderVisibility.ShowHeaderNumberAndName
                    : TableHeaderVisibility.ShowHeaderName;
                
                if (_selectedTab == null) return;
                
                _tableVisualizer.CurrentTable.TableAttributes.rowHeaderVisibility = TableSettings.GetSettings().rowHeaderVisibility;
                _tableVisualizer.CurrentTable.RefreshHeaderNames();
            });
            
            _removeFormulaOnCellChangeToggle.OnValueChanged += (value =>
            {
                TableSettings.GetSettings().removeFormulaOnCellValueChange = value;
            });
            
            _pollingToggle.OnValueChanged += (value =>
            {
                TableSettings.GetSettings().enablePolling = value;
                _pollingIntervalField.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            _pollingIntervalField.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue <= TableSettings.MinPollingInterval)
                {
                    _pollingIntervalField.SetValueWithoutNotify(TableSettings.MinPollingInterval);
                    TableSettings.GetSettings().pollingInterval = TableSettings.MinPollingInterval;
                    return;
                }
                
                TableSettings.GetSettings().pollingInterval = evt.newValue;
            });
            
            _adjustSizeButton.clicked += () =>
            {
                if (_selectedTab == null) return;
                
                _tableVisualizer.CurrentTable.Metadata.ClearAnchorSizes();
                _tableVisualizer.CurrentTable.RebuildPage(false);
            };
        }
        
        public void RefreshFunctionTextField()
        {
            if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null
                || _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell() is SubTableCell)
            {
                _functionTextField.RemoveFromChildrenClassList(TableVisualizerUss.ToolbarIncorrectFunctionField);
                _functionTextField.SetValueWithoutNotify("");
                _functionTextField.SetEnabled(false);
                return;
            }
            
            Cell focusedCell = _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell();
            string function = _selectedTab.GetFunction(focusedCell.Id);
            
            if(_tableVisualizer.CurrentTable.FunctionExecutor.IsCellFunctionCorrect(focusedCell.Id))
            {
                _functionTextField.RemoveFromChildrenClassList(TableVisualizerUss.ToolbarIncorrectFunctionField);
            }
            else
            {
                _functionTextField.AddToChildrenClassList(TableVisualizerUss.ToolbarIncorrectFunctionField);
            }
            
            _functionTextField.SetEnabled(true);
            _functionTextField.SetValueWithoutNotify(function ?? string.Empty);
        }
        
        private void RefreshCurrentCellLabel()
        {
            if (_selectedTab == null || _tableVisualizer.CurrentTable?.CellSelector.GetFocusedCell() == null)
            {
                _currentCellLabel.text = string.Empty;
                return;
            }
            
            Cell focusedCell = _tableVisualizer.CurrentTable.CellSelector.GetFocusedCell();
            _currentCellLabel.text = $"{focusedCell.GetGlobalPosition()}";
        }

        private void OnFocusedCellChanged()
        {
            RefreshFunctionTextField();
            RefreshCurrentCellLabel();
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
                previousTab.RemoveFromClassList(TableVisualizerUss.ToolbarTabSelected);
            }
            if (tableMetadata != null && _tabControls.TryGetValue(tableMetadata, out var newTab))
            {
                newTab.AddToClassList(TableVisualizerUss.ToolbarTabSelected);
            }
            
            if(_tableVisualizer.CurrentTable != null)
            {
                _tableVisualizer.CurrentTable.CellSelector.OnFocusedCellChanged -= OnFocusedCellChanged;
            }
            
            _selectedTab = tableMetadata;
            Table table = GetTable(tableMetadata);
            _tableVisualizer.SetTable(table);
            
            if(_tableVisualizer.CurrentTable != null)
            {
                _tableVisualizer.CurrentTable.CellSelector.OnFocusedCellChanged += OnFocusedCellChanged;
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