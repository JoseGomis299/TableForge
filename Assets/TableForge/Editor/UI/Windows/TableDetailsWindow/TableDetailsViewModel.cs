using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TableDetailsViewModel
    {
        public event Action OnTreeUpdated;
        
        private int _idCounter;
        private readonly Dictionary<string, HashSet<Type>> _namespaceTypes = new();
        
        protected readonly HashSet<ScriptableObject> SelectedAssets = new();
        protected readonly Dictionary<string, Type> AvailableTypes = new();
        protected readonly HashSet<string> TypeNames = new();
        protected string SelectedNamespace;
        
        public bool HasErrors { get; protected set; }
        public string TableName { get; protected set; }
        public bool UsePathsMode { get; set; }
        public Type SelectedType { get; protected set; }
        public List<TreeItem> TreeItems { get; } = new();
        
        public virtual string GetErrors()
        {
            HasErrors = true;
            
            if(string.IsNullOrEmpty(SelectedNamespace))
            {
                return "No namespace selected.";
            }
            
            if(SelectedType == null)
            {
                return "No type selected.";
            }
            
            if(UsePathsMode && SelectedAssets.Count == 0)
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
            SelectedAssets.Clear();
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
                Id = GetUniqueId(),
                Name = "Assets",
                IsFolder = true,
                Asset = null,
                Parent = null,
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
                            Id = GetUniqueId(),
                            Name = parts[i],
                            IsFolder = !isLeaf,
                            Asset = isLeaf ? asset : null,
                            Parent = parent,
                            IsSelected = isLeaf && SelectedAssets.Contains(asset),
                        };
                        parent.Children.Add(node);

                        if (!isLeaf) folderMap[curr] = node;
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

            AvailableTypes.Clear();

            var orderedTypes = _namespaceTypes[SelectedNamespace].OrderBy(t => t.Name).ToList();
            foreach (var type in orderedTypes)
            {
                TypeNames.Add(type.Name);
                AvailableTypes[type.Name] = type;
            }

            if (SelectedType == null || !_namespaceTypes[SelectedNamespace].Contains(SelectedType))
            {
                SelectedType = orderedTypes.FirstOrDefault();
            }
            
            typeDropdown.choices = orderedTypes.ConvertAll(t => t.Name);
            typeDropdown.SetValueWithoutNotify(SelectedType?.Name);
        }
        
        public void PopulateNamespaceDropdown(DropdownField namespaceDropdown)
        {
            namespaceDropdown.choices.Clear();
            namespaceDropdown.SetValueWithoutNotify(string.Empty);

            var namespaceSet = new HashSet<string>();
            bool globalNamespace = false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if(IsTypeInvalid(type)) continue;

                    string assetNamespace = type.Namespace;
                    if (string.IsNullOrEmpty(assetNamespace))
                    {
                        globalNamespace = true;
                        _namespaceTypes.TryAdd("Global", new HashSet<Type>());
                        _namespaceTypes["Global"].Add(type);
                    }
                    else
                    {
                        namespaceSet.Add(assetNamespace);
                        _namespaceTypes.TryAdd(assetNamespace, new HashSet<Type>());
                        _namespaceTypes[assetNamespace].Add(type);
                    }
                }
            }

            var namespaces = namespaceSet.OrderBy(n => n).ToList();
            if (globalNamespace) namespaces.Insert(0, "Global");
            
            if (string.IsNullOrEmpty(SelectedNamespace) || !namespaces.Contains(SelectedNamespace))
            {
                SelectedNamespace = namespaces.FirstOrDefault();
            }

            namespaceDropdown.choices = namespaces;
            namespaceDropdown.SetValueWithoutNotify(SelectedNamespace);
        }

        public void OnItemSelected(TreeItem item, bool selected)
        {
            if (selected) SelectedAssets.Add(item.Asset);
            else SelectedAssets.Remove(item.Asset);
            item.GetRoot().UpdateSelectionState();
        }
        
        public void OnTypeDropdownValueChanged(ChangeEvent<string> evt)
        {
            SelectedType = AvailableTypes.GetValueOrDefault(evt.newValue);
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
            SelectedNamespace = evt.newValue;
            PopulateTypeDropdown(typeDropdown);
        }
        
        public void CreateNewAssetsInFolder(TreeItem itemData, uint count)
        {
            string path = itemData.Name;
            TreeItem parent = itemData.Parent;
            while (parent != null)
            {
                path = parent.Name + "/" + path;
                parent = parent.Parent;
            }

            for (uint i = 0; i < count; i++)
            {
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + SelectedType.Name + ".asset");
                var asset = ScriptableObject.CreateInstance(SelectedType);
                AssetDatabase.CreateAsset(asset, assetPath);
                SelectedAssets.Add(asset);
                
                var item = new TreeItem
                {
                    Id = GetUniqueId(),
                    Name = asset.name,
                    IsFolder = false,
                    Asset = asset,
                    Parent = itemData,
                    IsSelected = true,
                };
                itemData.Children.Add(item);
            }
           
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshTree();
        }
        
        private bool IsTypeInvalid(Type type)
        {
            return !type.IsSubclassOf(typeof(ScriptableObject)) ||
                   type.IsAbstract || type.IsGenericType ||
                   type.Assembly == Assembly.GetAssembly(GetType()) ||
                   type.IsNotPublic ||
                   (!ToolbarData.EnableUnityTypesTables && IsUnityType(type));
        }

        private bool IsUnityType(Type type)
        {
            string assemblyName = type.Assembly.GetName().Name;
            return assemblyName.StartsWith("Unity")|| assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("UnityEditor");
        }

        private int GetUniqueId() => _idCounter++;

        private void StoreOpenItems()
        {
            
        }
    }
}