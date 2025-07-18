using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_6_OR_NEWER
// ReSharper disable once RedundantUsingDirective
using UnityEditor.UIElements; // For Unity versions before 6.0, use the legacy TreeView
#endif
using TableForge.Editor.UI.UssClasses;

namespace TableForge.Editor.UI
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
                var sortedItems = value.OrderByDescending(item => item.isFolder).ToList();

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
                name = TableDetailsUss.AssetTree,
                selectionType = SelectionType.Multiple,
                fixedItemHeight = 20,
                reorderable = false,
            };
            _treeView.AddToClassList(TableDetailsUss.AssetTree);
#if UNITY_6000_0_OR_NEWER
                        _treeView.canStartDrag += _ => false;
#endif
            _treeView.makeItem = MakeTreeItem;
            _treeView.bindItem = BindTreeItem;
            _treeView.unbindItem = UnbindTreeItem;
            _treeView.viewDataKey = "asset-tree-view";
            Add(_treeView);
        }

        private VisualElement MakeTreeItem()
        {
            var container = new VisualElement();
            container.AddToClassList(TableDetailsUss.TreeItemContainer);
            container.pickingMode = PickingMode.Ignore;
            
            var toggle = new Toggle { name = TableDetailsUss.ItemToggle };
            toggle.AddToClassList(TableDetailsUss.ItemToggle);
            container.Add(toggle);

            var label = new Label { name = TableDetailsUss.ItemLabel };
            label.AddToClassList(TableDetailsUss.ItemLabel);
            container.Add(label);

            var textField = new TextField { name = TableDetailsUss.ItemTextField };
            textField.AddToClassList(TableDetailsUss.ItemTextField);
            container.Add(textField);

            var itemCount = new UnsignedIntegerField { name = TableDetailsUss.ItemCount, label = "count", maxLength = 2, value = 1 };
            itemCount.AddToClassList(TableDetailsUss.ItemCount);
            var itemCountLabel = itemCount.Q<Label>();
            if (itemCountLabel != null) itemCountLabel.style.minWidth = 0;
            container.Add(itemCount);

            var addButton = new Button { name = TableDetailsUss.AddButton, text = "+" };
            addButton.AddToClassList(TableDetailsUss.AddButton);
            container.Add(addButton);

            container.AddManipulator(new ContextualMenuManipulator(context =>
            {
                var itemData = container.userData as TreeItem;
                if (itemData != null && !itemData.isFolder)
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
                        _detailsViewModel.DeleteAsset(itemData.asset);
                    });
                }
            }));
            return container;
        }

        private void BindTreeItem(VisualElement element, int index)
        {
            var itemData = _treeView.GetItemDataForIndex<TreeItem>(index);
            element.userData = itemData;
            itemData.element = element;
            itemData.element.parent.style.alignSelf = Align.Center;
    
            var toggle = element.Q<Toggle>(TableDetailsUss.ItemToggle);
            var label = element.Q<Label>(TableDetailsUss.ItemLabel);
            var addButton = element.Q<Button>(TableDetailsUss.AddButton);
            var itemCount = element.Q<UnsignedIntegerField>(TableDetailsUss.ItemCount);
            var textField = element.Q<TextField>(TableDetailsUss.ItemTextField);
            
            EventCallback<EventBase> textFieldCallback = evt =>
            {
                if(evt is KeyDownEvent keyDownEvent && keyDownEvent.keyCode != KeyCode.Return && keyDownEvent.keyCode != KeyCode.KeypadEnter)
                    return;
                
                string path = AssetDatabase.GetAssetPath(itemData.asset);
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
                itemData.isSelected = evt.newValue;
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

            label.text = itemData.isFolder ? itemData.name : itemData.name.Remove(itemData.name.Length - 6); // Remove ".asset"
            if (itemData.isFolder)
            {
                label.AddToClassList(TableDetailsUss.ItemLabelFolder);
                itemCount.visible = true;
                itemCount.style.display = DisplayStyle.Flex;
                addButton.style.display = DisplayStyle.Flex;
                if(!_buttonCallbacks.TryAdd(addButton, () =>
                   {
                       _detailsViewModel.CreateNewAssetsInFolder(itemData, itemCount.value);
                       OnSelectionChanged?.Invoke();
                   }))
                {
                    addButton.clicked -= _buttonCallbacks[addButton];
                }
                addButton.clicked += _buttonCallbacks[addButton];
            }
            else
            {
                label.RemoveFromClassList(TableDetailsUss.ItemLabelFolder);
                itemCount.visible = false;
                itemCount.style.display = DisplayStyle.None;
                addButton.style.display = DisplayStyle.None;
            }
            UpdateVisualState(element, itemData);
            if(itemData.isSelected)
            {
                toggle.SetValueWithoutNotify(true);
            }
            else
            {
                if(itemData.isPartiallySelected && itemData.isFolder)
                    toggle.showMixedValue = true;
                else
                    toggle.SetValueWithoutNotify(false);
            }
        }
        
        private void UnbindTreeItem(VisualElement element, int index)
        {
            var itemData = _treeView.GetItemDataForIndex<TreeItem>(index);
            if(itemData == null) return;
            itemData.element = null;
        }

        private void UpdateVisualState(VisualElement element, TreeItem item)
        {
            var toggle = element.Q<Toggle>(TableDetailsUss.ItemToggle);
            var label = element.Q<Label>();

            toggle.SetValueWithoutNotify(item.isSelected);
            if (item.isFolder)
            {
                toggle.visible = true;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                toggle.showMixedValue = item.isPartiallySelected;

                foreach (var child in item.children)
                {
                    if(child.element != null)
                    {
                        UpdateVisualState(child.element, child);
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
            item.isSelected = selected;
            item.isPartiallySelected = false;
            

            if (item.isFolder)
            {
                foreach (var child in item.children)
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
            var parent = item.parent;
            while (parent != null)
            {
                parent.UpdateSelectionState();
                if(parent.element != null)
                {
                    UpdateVisualState(parent.element, parent);
                }
                
                parent = parent.parent;
            }
        }
        
        private TreeViewItemData<TreeItem> ConvertToTreeViewItem(TreeItem item)
        {
            return new TreeViewItemData<TreeItem>(
                item.id, 
                item,
                item.children.OrderByDescending(x => x.isFolder).Select(ConvertToTreeViewItem).ToList()
            );
        }
    }
}