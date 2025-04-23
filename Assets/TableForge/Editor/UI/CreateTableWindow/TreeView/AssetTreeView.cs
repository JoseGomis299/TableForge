using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    public class AssetTreeView : VisualElement
    {
        public event Action<TreeItem, bool> OnItemSelectionChanged;

        private readonly TreeView _treeView;
        private readonly Dictionary<Toggle, EventCallback<ChangeEvent<bool>>> _toggleCallbacks = new();

        public List<TreeItem> ItemsSource
        {
            set
            {
                _treeView.Clear();
                _treeView.SetRootItems(value.Select(ConvertToTreeViewItem).ToList());
                _treeView.Rebuild();
            }
        }

        public AssetTreeView()
        {
            _treeView = new TreeView
            {
                name = "asset-tree",
                selectionType = SelectionType.Multiple,
                itemHeight = 20
            };

            _treeView.makeItem = MakeTreeItem;
            _treeView.bindItem = BindTreeItem;
            _treeView.unbindItem = UnbindTreeItem;

            Add(_treeView);
        }

        private VisualElement MakeTreeItem()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            container.Add(new Toggle { name = "item-toggle", style = { width = 20 } });
            container.Add(new Label { style = { flexGrow = 1 } });
            return container;
        }

        private void BindTreeItem(VisualElement element, int index)
        {
            var itemData = _treeView.GetItemDataForIndex<TreeItem>(index);
            itemData.Element = element;
            var toggle = element.Q<Toggle>("item-toggle");
            var label = element.Q<Label>();

            toggle.SetValueWithoutNotify(itemData.IsSelected);
            EventCallback<ChangeEvent<bool>> callback = evt =>
            {
                itemData.IsSelected = evt.newValue;
                UpdateChildrenSelection(itemData, evt.newValue);
                UpdateParentSelections(itemData);
                UpdateVisualState(element, itemData);
            };
            
            if(!_toggleCallbacks.TryAdd(toggle, callback))
            {
                toggle.UnregisterValueChangedCallback(_toggleCallbacks[toggle]);
                _toggleCallbacks[toggle] = callback;
            }
            toggle.RegisterValueChangedCallback(_toggleCallbacks[toggle]);

            label.text = itemData.Name;
            label.style.marginLeft = itemData.IsFolder ? 0 : 20;
            UpdateVisualState(element, itemData);
        }
        
        private void UnbindTreeItem(VisualElement element, int index)
        {
            var itemData = _treeView.GetItemDataForIndex<TreeItem>(index);
            if(itemData == null) return;
            itemData.Element = null;
        }

        private void UpdateVisualState(VisualElement element, TreeItem item)
        {
            var toggle = element.Q<Toggle>("item-toggle");
            var label = element.Q<Label>();

            toggle.SetValueWithoutNotify(item.IsSelected);
            if (item.IsFolder)
            {
                toggle.visible = true;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                toggle.showMixedValue = item.IsPartiallySelected;

                foreach (var child in item.Children)
                {
                    if(child.Element != null)
                    {
                        UpdateVisualState(child.Element, child);
                    }
                }
            }
            else
            {
                toggle.showMixedValue = false;
                label.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
        }

        private void UpdateChildrenSelection(TreeItem item, bool selected)
        {
            item.IsSelected = selected;
            item.IsPartiallySelected = false;
            

            if (item.IsFolder)
            {
                foreach (var child in item.Children)
                {
                    UpdateChildrenSelection(child, selected);
                }
            }
            else
            {
                OnItemSelectionChanged?.Invoke(item, selected);
            }
        }

        private void UpdateParentSelections(TreeItem item)
        {
            var parent = item.Parent;
            while (parent != null)
            {
                UpdateParentSelectionState(parent);
                if(parent.Element != null)
                {
                    UpdateVisualState(parent.Element, parent);
                }
                
                parent = parent.Parent;
            }
        }

        private void UpdateParentSelectionState(TreeItem parent)
        {
            if (!parent.IsFolder) return;

            float selectedCount = 0;
            int childCount = parent.Children.Count;
            
            foreach (var child in parent.Children)
            {
                if (child.IsSelected) selectedCount++;
                else if (child.IsPartiallySelected) selectedCount += 0.5f;
            }

            parent.IsPartiallySelected = selectedCount > 0 && selectedCount < childCount;
            parent.IsSelected = Mathf.Approximately(selectedCount, childCount);
        }

        private TreeViewItemData<TreeItem> ConvertToTreeViewItem(TreeItem item)
        {
            return new TreeViewItemData<TreeItem>(
                item.Id, 
                item,
                item.Children.Select(ConvertToTreeViewItem).ToList()
            );
        }
    }
}