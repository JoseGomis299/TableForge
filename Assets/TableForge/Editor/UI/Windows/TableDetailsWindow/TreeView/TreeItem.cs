using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal class TreeItem
    {
        public int id;
        public string name;
        public Object asset;
        public TreeItem parent;
        public readonly List<TreeItem> children = new();
        public bool isSelected;
        public bool isFolder;
        public bool isPartiallySelected;
        public VisualElement element;
        
        public void UpdateSelectionState()
        {
            if (!isFolder) return;

            float selectedCount = 0;
            int childCount = children.Count;
            
            foreach (var child in children)
            {
                if (child.isFolder)
                {
                    child.UpdateSelectionState();
                }
                
                if (child.isSelected) selectedCount++;
                else if (child.isPartiallySelected) selectedCount += 0.5f;
            }

            isPartiallySelected = selectedCount > 0 && selectedCount < childCount;
            isSelected = Mathf.Approximately(selectedCount, childCount);
        } 
        
        public TreeItem GetRoot()
        {
            TreeItem current = this;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current;
        }
    }
}