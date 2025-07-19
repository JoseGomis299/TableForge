using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TableForge.Editor.UI
{
    internal class TableDetailsViewModel
    {
        public event Action OnTreeUpdated;
        
        private int _idCounter;

        private readonly HashSet<string> _extraPaths = new();
        
        protected readonly HashSet<Object> selectedAssets = new();
        protected string selectedNamespace;
        
        public bool HasErrors { get; protected set; }
        public string TableName { get; set; }
        public bool UsePathsMode { get; set; }
        public Type SelectedType { get; protected set; }
        public List<TreeItem> TreeItems { get; } = new();
        
        public virtual string GetErrors()
        {
            HasErrors = true;
            
            if(string.IsNullOrEmpty(selectedNamespace))
            {
                return "No namespace selected.";
            }
            
            if(SelectedType == null)
            {
                return "No type selected.";
            }
            
            if(UsePathsMode && selectedAssets.Count == 0)
            {
                return "No assets selected.";
            }
            
            if(string.IsNullOrEmpty(TableName))
            {
                return "Table name cannot be empty.";
            }
            
            HasErrors = false;
            return string.Empty;
        }
        
        public void ClearSelectedAssets()
        {
            selectedAssets.Clear();
        }
        
        public void RefreshTree()
        {
            _idCounter = 0;
            TreeItems.Clear();
            var guids = AssetDatabase.FindAssets($"t:{SelectedType?.Name}");
            var folderMap = new Dictionary<string, TreeItem>();

            // Create the root "Assets" node
            var assetsRoot = new TreeItem
            {
                id = GetUniqueId(),
                name = "Assets",
                isFolder = true,
                asset = null,
                parent = null,
            };
            TreeItems.Add(assetsRoot);
            folderMap["Assets"] = assetsRoot;

            // Create the tree structure
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                var parts = path.Split('/').Skip(1).ToArray(); 
                TreeItem parent = assetsRoot; 
                string curr = "Assets";

                for (int i = 0; i < parts.Length; i++)
                {
                    curr += "/" + parts[i];
                    bool isLeaf = (i == parts.Length - 1);
                    if (!folderMap.TryGetValue(curr, out var node))
                    {
                        node = new TreeItem
                        {
                            id = GetUniqueId(),
                            name = parts[i],
                            isFolder = !isLeaf,
                            asset = isLeaf ? asset : null,
                            parent = parent,
                            isSelected = isLeaf && selectedAssets.Contains(asset),
                        };
                        parent.children.Add(node);

                        if (!isLeaf) folderMap[curr] = node;
                    }
                    parent = node;
                }
            }
            
            // Add extra paths
            foreach (var path in _extraPaths)
            {
                var parts = path.Split('/').Skip(1).ToArray();
                TreeItem parent = assetsRoot;
                string curr = "Assets";

                for (int i = 0; i < parts.Length; i++)
                {
                    curr += "/" + parts[i];
                    if (!folderMap.TryGetValue(curr, out var node))
                    {
                        node = new TreeItem
                        {
                            id = GetUniqueId(),
                            name = parts[i],
                            isFolder = true,
                            asset = null,
                            parent = parent,
                        };
                        parent.children.Add(node);

                        folderMap[curr] = node;
                    }
                    parent = node;
                }
            }
            
            //Set selection state
            foreach (var item in TreeItems)
            {
                item.UpdateSelectionState();
            }
            OnTreeUpdated?.Invoke();
        }
        
        public void PopulateTypeDropdown(DropdownField typeDropdown)
        {
            typeDropdown.choices.Clear();
            typeDropdown.SetValueWithoutNotify(string.Empty);

            var orderedTypes = TypeRegistry.NamespaceTypes[selectedNamespace].OrderBy(t => t.Name).ToList();
            if (SelectedType == null || !TypeRegistry.NamespaceTypes[selectedNamespace].Contains(SelectedType))
            {
                SelectedType = orderedTypes.FirstOrDefault();
            }
            
            typeDropdown.choices = orderedTypes.ConvertAll(t => t.Name);
            typeDropdown.SetValueWithoutNotify(SelectedType?.Name ?? string.Empty);
        }
        
        public void PopulateNamespaceDropdown(DropdownField namespaceDropdown)
        {
            namespaceDropdown.choices.Clear();
            namespaceDropdown.SetValueWithoutNotify(string.Empty);
            
            if (string.IsNullOrEmpty(selectedNamespace) || !TypeRegistry.Namespaces.Contains(selectedNamespace))
            {
                selectedNamespace = TypeRegistry.Namespaces.FirstOrDefault();
            }

            namespaceDropdown.choices = TypeRegistry.Namespaces.ToList();
            namespaceDropdown.SetValueWithoutNotify(selectedNamespace);
        }

        public void OnItemSelected(TreeItem item, bool selected)
        {
            if (selected) selectedAssets.Add(item.asset);
            else selectedAssets.Remove(item.asset);
            item.GetRoot().UpdateSelectionState();
        }
        
        public void OnTypeDropdownValueChanged(ChangeEvent<string> evt)
        {
            if(string.IsNullOrEmpty(selectedNamespace) || !TypeRegistry.TypesByNamespaceAndName.ContainsKey(selectedNamespace)) return;
            SelectedType = TypeRegistry.TypesByNamespaceAndName[selectedNamespace].GetValueOrDefault(evt.newValue);
        }
        
        public void OnNameFieldValueChanged(ChangeEvent<string> evt, TextField field)
        {
            string value = evt.newValue;
            
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length > 50)
                {
                    value = value.Substring(0, 50).Trim();
                    field.SetValueWithoutNotify(value);
                    return;
                }
                
                value = value.Trim();
            }
            
            TableName = value;
        }
        
        public void OnNamespaceDropdownValueChanged(ChangeEvent<string> evt, DropdownField typeDropdown)
        {
            selectedNamespace = evt.newValue;
            PopulateTypeDropdown(typeDropdown);
            typeDropdown.value = SelectedType?.Name ?? string.Empty;
            RefreshTree();
        }
        
        public void CreateNewAssetsInFolder(TreeItem itemData, uint count)
        {
            string path = itemData.name;
            TreeItem parent = itemData.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            for (uint i = 0; i < count; i++)
            {
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + SelectedType.Name + ".asset");
                var asset = ScriptableObject.CreateInstance(SelectedType);
                AssetDatabase.CreateAsset(asset, assetPath);
                selectedAssets.Add(asset);
                
                var item = new TreeItem
                {
                    id = GetUniqueId(),
                    name = asset.name,
                    isFolder = false,
                    asset = asset,
                    parent = itemData,
                    isSelected = true,
                };
                itemData.children.Add(item);
            }
           
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshTree();
        }
        
      
        public int GetUniqueId() => _idCounter++;

        public void AddPathToTree(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            
            _extraPaths.Add(path);
            RefreshTree();
        }

        public void DeleteAsset(Object asset)
        {
            if (asset == null) return;

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            if (AssetUtils.DeleteAsset(guid))
            {
                selectedAssets.Remove(asset);
                RefreshTree();
            }
        }
    }
}