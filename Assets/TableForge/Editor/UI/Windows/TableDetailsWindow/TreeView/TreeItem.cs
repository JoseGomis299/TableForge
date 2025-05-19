using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal class TreeItem
    {
        public int Id;
        public string Name;
        public Object Asset;
        public TreeItem Parent;
        public List<TreeItem> Children = new List<TreeItem>();
        public bool IsSelected;
        public bool IsFolder;
        public bool IsPartiallySelected;
        public VisualElement Element;
        
        public void UpdateSelectionState()
        {
            if (!IsFolder) return;

            float selectedCount = 0;
            int childCount = Children.Count;
            
            foreach (var child in Children)
            {
                if (child.IsFolder)
                {
                    child.UpdateSelectionState();
                }
                
                if (child.IsSelected) selectedCount++;
                else if (child.IsPartiallySelected) selectedCount += 0.5f;
            }

            IsPartiallySelected = selectedCount > 0 && selectedCount < childCount;
            IsSelected = Mathf.Approximately(selectedCount, childCount);
        } 
        
        public TreeItem GetRoot()
        {
            TreeItem current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
            }
            return current;
        }
    }
}