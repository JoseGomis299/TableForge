using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class AssetTreeView : VisualElement
    {
        public event Action<TreeItem, bool> OnItemSelectionChanged;
        public event Action OnSelectionChanged; 

        private readonly TreeView _treeView;
        private readonly TableDetailsViewModel _detailsViewModel;
        
        private readonly Dictionary<Toggle, EventCallback<ChangeEvent<bool>>> _toggleCallbacks = new();
        private readonly Dictionary<TextField, EventCallback<EventBase>> _textFieldFocusOutCallbacks = new();
        private readonly Dictionary<TextField, EventCallback<EventBase>> _textFieldKeyDownCallbacks = new();
        private readonly Dictionary<Button, Action> _buttonCallbacks = new();

        public List<TreeItem> ItemsSource
        {
            set
            {
                var sortedItems = value.OrderByDescending(item => item.IsFolder).ToList();

                _treeView.Clear();
                _treeView.SetRootItems(sortedItems.Select(ConvertToTreeViewItem).ToList());
                _treeView.Rebuild();
            }
        }

        public AssetTreeView(TableDetailsViewModel detailsViewModel)
        {
            _detailsViewModel = detailsViewModel;
            _treeView = new TreeView
            {
                name = "asset-tree",
                selectionType = SelectionType.Multiple,
                fixedItemHeight = 20
            };
            
            _treeView.makeItem = MakeTreeItem;
            _treeView.bindItem = BindTreeItem;
            _treeView.unbindItem = UnbindTreeItem;
            _treeView.viewDataKey = "asset-tree-view";

            Add(_treeView);
        }

        private VisualElement MakeTreeItem()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center} };
            container.Add(new Toggle { name = "item-toggle", style = { width = 20, marginBottom = 1, height = container.style.height} });

            var label = new Label { name = "item-label", style = { flexGrow = 1,  } };
            var textField = new TextField { name = "item-textfield", style = { flexGrow = 1, display = DisplayStyle.None } };

            container.Add(label);
            container.Add(textField);

            var itemCount = new UnsignedIntegerField { name = "item-count", label = "count", maxLength = 2, value = 1, style = { width = 60, display = DisplayStyle.None } };
            var itemCountLabel = itemCount.Q<Label>();
            itemCountLabel.style.minWidth = 0;

            var addButton = new Button { name = "add-button", text = "+", style = { width = 20, display = DisplayStyle.None } };
            container.Add(itemCount);
            container.Add(addButton);
            
            container.AddManipulator(new ContextualMenuManipulator(context =>
            {
                var itemData = container.userData as TreeItem;
                if (itemData != null && !itemData.IsFolder)
                {
                    context.menu.AppendAction("Rename", (_) =>
                    {
                        textField.value = label.text;
                        label.style.display = DisplayStyle.None;
                        textField.style.display = DisplayStyle.Flex;
                        textField.Focus();
                    });
                    
                    context.menu.AppendAction("Delete", (_) =>
                    {
                        _detailsViewModel.DeleteAsset(itemData.Asset);
                    });
                }
            }));
            
            return container;
        }

        private void BindTreeItem(VisualElement element, int index)
        {
            var itemData = _treeView.GetItemDataForIndex<TreeItem>(index);
            element.userData = itemData;
            itemData.Element = element;
            itemData.Element.parent.style.alignSelf = Align.Center;
    
            var toggle = element.Q<Toggle>("item-toggle");
            var label = element.Q<Label>("item-label");
            var addButton = element.Q<Button>("add-button");
            var itemCount = element.Q<UnsignedIntegerField>("item-count");
            var textField = element.Q<TextField>("item-textfield");
            
            EventCallback<EventBase> textFieldCallback = _ =>
            {
                string path = AssetDatabase.GetAssetPath(itemData.Asset);
                string newName = AssetUtils.RenameAsset(path, textField.value.Trim());
                
                label.text = newName;
                textField.value = newName;
                textField.style.display = DisplayStyle.None;
                label.style.display = DisplayStyle.Flex;
            };
            
            if(!_textFieldFocusOutCallbacks.TryAdd(textField, textFieldCallback))
            {
                textField.UnregisterCallback<FocusOutEvent>(_textFieldFocusOutCallbacks[textField]);
                _textFieldFocusOutCallbacks[textField] = textFieldCallback;
            }
            textField.RegisterCallback<FocusOutEvent>(_textFieldFocusOutCallbacks[textField]);
            
            if(!_textFieldKeyDownCallbacks.TryAdd(textField, textFieldCallback))
            {
                textField.UnregisterCallback<KeyDownEvent>(_textFieldKeyDownCallbacks[textField]);
                _textFieldKeyDownCallbacks[textField] = textFieldCallback;
            }
            textField.RegisterCallback<KeyDownEvent>(_textFieldKeyDownCallbacks[textField]);

            EventCallback<ChangeEvent<bool>> toggleCallback = evt =>
            {
                itemData.IsSelected = evt.newValue;
                UpdateChildrenSelection(itemData, evt.newValue);
                UpdateParentSelections(itemData);
                UpdateVisualState(element, itemData);
                OnSelectionChanged?.Invoke();
            };

            if(!_toggleCallbacks.TryAdd(toggle, toggleCallback))
            {
                toggle.UnregisterValueChangedCallback(_toggleCallbacks[toggle]);
                _toggleCallbacks[toggle] = toggleCallback;
            }
            toggle.RegisterValueChangedCallback(_toggleCallbacks[toggle]);

            label.text = itemData.IsFolder ? itemData.Name : itemData.Name.Remove(itemData.Name.Length - 6); // Remove ".asset"
            label.style.marginLeft = itemData.IsFolder ? 0 : 20;

            if (itemData.IsFolder)
            {
                itemCount.visible = true;
                itemCount.style.display = DisplayStyle.Flex;
                addButton.style.display = DisplayStyle.Flex;
                if(!_buttonCallbacks.TryAdd(addButton, () => _detailsViewModel.CreateNewAssetsInFolder(itemData, itemCount.value)))
                {
                    addButton.clicked -= _buttonCallbacks[addButton];
                }
                addButton.clicked += _buttonCallbacks[addButton];
            }
            else
            {
                itemCount.visible = false;
                itemCount.style.display = DisplayStyle.None;
                addButton.style.display = DisplayStyle.None;
            }

            UpdateVisualState(element, itemData);
            
            if(itemData.IsSelected)
            {
                toggle.SetValueWithoutNotify(true);
            }
            else
            {
                if(itemData.IsPartiallySelected && itemData.IsFolder)
                    toggle.showMixedValue = true;
                else
                    toggle.SetValueWithoutNotify(false);
            }
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
                parent.UpdateSelectionState();
                if(parent.Element != null)
                {
                    UpdateVisualState(parent.Element, parent);
                }
                
                parent = parent.Parent;
            }
        }
        
        private TreeViewItemData<TreeItem> ConvertToTreeViewItem(TreeItem item)
        {
            return new TreeViewItemData<TreeItem>(
                item.Id, 
                item,
                item.Children.OrderByDescending(x => x.IsFolder).Select(ConvertToTreeViewItem).ToList()
            );
        }
    }
}