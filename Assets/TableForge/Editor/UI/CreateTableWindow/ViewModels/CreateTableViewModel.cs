using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    public class CreateTableViewModel
    {
        public bool UsePathsMode { get; set; }
        public Type SelectedType { get; set; }
        public List<TreeItem> TreeItems { get; private set; } = new();
        public HashSet<ScriptableObject> SelectedAssets { get; private set; } = new();
        private Dictionary<string, Type> availableTypes = new();
        private string _selectedNamespace;

        public void RefreshTree()
        {
            TreeItems.Clear();
            SelectedAssets.Clear();
            var guids = AssetDatabase.FindAssets($"t:{SelectedType?.Name}");
            var folderMap = new Dictionary<string, TreeItem>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                var parts = path.Split('/').Skip(1).ToArray();
                TreeItem parent = null;
                string curr = "Assets";

                for (int i = 0; i < parts.Length; i++)
                {
                    curr += "/" + parts[i];
                    bool isLeaf = (i == parts.Length - 1);
                    if (!folderMap.TryGetValue(curr, out var node))
                    {
                        node = new TreeItem
                        {
                            Id = GetUniqueId(),
                            Name = parts[i],
                            IsFolder = !isLeaf,
                            Asset = isLeaf ? asset : null,
                            Parent = parent
                        };
                        if (parent == null) TreeItems.Add(node);
                        else parent.Children.Add(node);

                        if (!isLeaf) folderMap[curr] = node;
                    }
                    parent = node;
                }
            }
        }

        public void OnItemSelected(TreeItem item, bool selected)
        {
            if (selected) SelectedAssets.Add(item.Asset);
            else SelectedAssets.Remove(item.Asset);
            UpdateParents(item);
        }

        private void UpdateParents(TreeItem item)
        {
            var parent = item.Parent;
            while (parent != null)
            {
                int total = parent.Children.Count;
                int sel = parent.Children.Count(c => c.IsSelected) +
                          parent.Children.Count(c => c.IsPartiallySelected) / 2;
                parent.IsSelected = sel == total;
                parent.IsPartiallySelected = sel > 0 && sel < total;
                parent = parent.Parent;
            }
        }

        public bool CanConfirm() =>
            SelectedType != null && (!UsePathsMode || SelectedAssets.Count > 0);

        public void CreateTable()
        {
            var result = UsePathsMode
                ? SelectedAssets.ToArray()
                : AssetDatabase.FindAssets($"t:{SelectedType.Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(p => AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(p))
                    .ToArray();

            UnityEngine.Debug.Log($"Selected {result.Length} assets");
        }
        
        public void PopulateTypeDropdown(DropdownField typeDropdown)
        {
            // Clear existing choices
            typeDropdown.choices.Clear();
            availableTypes.Clear();
            typeDropdown.value = string.Empty;
            
            // Find all ScriptableObject assets in the project
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            var typeSet = new HashSet<Type>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                Type assetType = asset.GetType();
        
                // Filter out abstract classes, generic types, and base ScriptableObject type
                if (!assetType.IsAbstract && 
                    !assetType.IsGenericType && 
                    assetType != typeof(ScriptableObject)
                    && (string.IsNullOrEmpty(_selectedNamespace) || assetType.Namespace == _selectedNamespace || (assetType.Namespace == null && _selectedNamespace == "Global")))
                {
                    typeSet.Add(assetType);
                    availableTypes.TryAdd(assetType.Name, assetType);
                }
            }

            // Convert to sorted list
            var soTypes = typeSet.OrderBy(t => t.Name).ToList();

            // Populate dropdown
            typeDropdown.choices = soTypes.ConvertAll(t => t.Name);
            typeDropdown.value = soTypes.FirstOrDefault()?.Name;
            SelectedType = soTypes.FirstOrDefault();
        }
        
        public void OnTypeDropdownValueChanged(ChangeEvent<string> evt)
        {
            SelectedType = availableTypes.GetValueOrDefault(evt.newValue);
        }
        
        public void PopulateNamespaceDropdown(DropdownField namespaceDropdown)
        {
            // Clear existing choices
            namespaceDropdown.choices.Clear();
            availableTypes.Clear();
            namespaceDropdown.value = string.Empty;
            bool globalNamespace = false;
            
            // Find all ScriptableObject assets in the project
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            var namespaceSet = new HashSet<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                Type assetType = asset.GetType();
        
                // Filter out abstract classes, generic types, and base ScriptableObject type
                if (!assetType.IsAbstract && 
                    !assetType.IsGenericType && 
                    assetType != typeof(ScriptableObject)
                    && assetType.Assembly != Assembly.GetAssembly(GetType()))
                {
                    string assetNamespace = assetType.Namespace;
                    if (string.IsNullOrEmpty(assetNamespace))
                    {
                        globalNamespace = true;
                        continue;
                    }
                    
                    namespaceSet.Add(assetNamespace);
                }
            }

            // Convert to sorted list
            var namespaces = namespaceSet.OrderBy(n => n).ToList();
            if(globalNamespace) namespaces.Insert(0, "Global");

            // Populate dropdown
            namespaceDropdown.choices = namespaces;
            namespaceDropdown.value = namespaces.FirstOrDefault();
            _selectedNamespace = namespaces.FirstOrDefault();
        }
        
        public void OnNamespaceDropdownValueChanged(ChangeEvent<string> evt, DropdownField typeDropdown)
        {
            _selectedNamespace = evt.newValue;
            PopulateTypeDropdown(typeDropdown);
        }

        private static int _idCounter;
        private static int GetUniqueId() => _idCounter++;
        
    }
}