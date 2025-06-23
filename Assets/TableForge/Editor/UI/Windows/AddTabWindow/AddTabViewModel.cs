using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class AddTabViewModel
    {
        public event Action<TabSelectionButton, bool> OnTabSelectionChanged;
        
        private readonly ToolbarController _toolbarController;
        private readonly Dictionary<TableMetadata, TabSelectionButton> _tabButtons = new();

        private HashSet<TableMetadata> OpenTables { get; } = new();

        public AddTabViewModel(ToolbarController toolbarController)
        {
            _toolbarController = toolbarController;
        }
        
        public void PopulateTabContainers(VisualElement existingTablesContainer, VisualElement openTabsContainer)
        {
            existingTablesContainer.Clear();
            openTabsContainer.Clear();
            List<TableMetadata> existingTables = TableMetadataManager.GetAllMetadata();
            IEnumerable<TableMetadata> openedTables = _toolbarController.OpenTabs;

            foreach (var table in openedTables)
            {
                OpenTables.Add(table);  
                openTabsContainer.Add(CreateTabButton(table, openTabsContainer));
            }
            
            foreach (var table in existingTables)
            {
                if (OpenTables.Contains(table)) continue;
                CreateTabButton(table, existingTablesContainer);
            }
        }
        
        public void UpdateTabContainers(VisualElement existingTablesContainer, VisualElement openTabsContainer)
        {
            existingTablesContainer.Clear();
            openTabsContainer.Clear();
            
            foreach (var table in OpenTables)
            {
                openTabsContainer.Add(CreateTabButton(table, openTabsContainer));
            }

            List<TableMetadata> existingTables = TableMetadataManager.GetAllMetadata();
            foreach (var table in existingTables)
            {
                if (OpenTables.Contains(table)) continue;
                CreateTabButton(table, existingTablesContainer);
            }
        }

        public void ClearTabs()
        {
            List<TableMetadata> tablesToRemove = OpenTables.ToList();
            foreach (var table in tablesToRemove)
            {
                OpenTables.Remove(table);
            }
        }

        public void CreateNewTable(VisualElement container)
        {
            CreateTableViewModel viewModel = new CreateTableViewModel();
            viewModel.OnTableCreated += (table) => CreateTabButton(table, container);
            CreateTableWindow.ShowWindow(viewModel);
        }

        public void AddCurrentTabs()
        {
            UndoRedoManager.StartCollection();

            // Close all tabs that are not in the OpenTables list
            IEnumerable<TableMetadata> tabsToCheck = _toolbarController.OpenTabs.ToList();
            foreach (var openTab in tabsToCheck)
            {
                if (OpenTables.Contains(openTab)) continue;
                _toolbarController.CloseTab(openTab);
            }
            
            // Open all tabs that are in the OpenTables list
            foreach (var table in OpenTables)
            {
                _toolbarController.OpenTab(table);
            }

            UndoRedoManager.EndCollection();
        }
        
        public void ToggleTab(TabSelectionButton tab)
        {
            bool opened = OpenTables.Contains(tab.TableMetadata);
            
            if (!opened)
            {
                OpenTables.Add(tab.TableMetadata);
                OnTabSelectionChanged?.Invoke(tab, true);
            }
            else
            {
                OpenTables.Remove(tab.TableMetadata);
                OnTabSelectionChanged?.Invoke(tab, false);
            }
        }
        
        public void DeleteTab(TableMetadata tableMetadata)
        {
            AssetUtils.DeleteAsset(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(tableMetadata)).ToString(), () =>
            {
                if (_tabButtons.TryGetValue(tableMetadata, out var button))
                {
                    button.RemoveFromHierarchy();
                    _tabButtons.Remove(tableMetadata);
                }
            
                if (OpenTables.Contains(tableMetadata))
                {
                    OpenTables.Remove(tableMetadata);
                }
            
                _toolbarController.CloseTab(tableMetadata);
            });
        }
        
        public bool IsTabOpen(TableMetadata tableMetadata)
        {
            return OpenTables.Contains(tableMetadata);
        }

        private TabSelectionButton CreateTabButton(TableMetadata table, VisualElement parent)
        {
            if (_tabButtons.TryGetValue(table, out var existingButton))
            {
                parent.Add(existingButton);
                return existingButton;
            }
            
            TabSelectionButton button = new TabSelectionButton(table, this);
            _tabButtons.Add(table, button);
            parent.Add(button);
            return button;
        }
    }
}