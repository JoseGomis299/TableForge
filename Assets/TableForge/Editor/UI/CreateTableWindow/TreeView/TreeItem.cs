using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    public class TreeItem
    {
        public int Id;
        public string Name;
        public ScriptableObject Asset;
        public TreeItem Parent;
        public List<TreeItem> Children = new List<TreeItem>();
        public bool IsSelected;
        public bool IsFolder;
        public bool IsPartiallySelected;
        public VisualElement Element;
    }
}