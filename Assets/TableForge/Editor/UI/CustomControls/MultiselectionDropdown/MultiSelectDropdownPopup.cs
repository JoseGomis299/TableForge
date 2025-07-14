using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI.CustomControls
{
    internal class MultiSelectDropdownPopup : EditorWindow
    {
        private List<DropdownElement> _allItems;
        private HashSet<int> _selectedItems;
        private Action<List<DropdownElement>> _onClose;
        private Vector2 _scrollPos;

        private const float ItemHeight = 18f;
        private const float MaxHeight = 200f;
        
        public bool IsOpen { get; private set; }

        public static MultiSelectDropdownPopup Show(List<DropdownElement> allItems, List<DropdownElement> currentSelection, MultiSelectDropdownButton activator, Action<List<DropdownElement>> onClose)
        {
            var window = CreateInstance<MultiSelectDropdownPopup>();
            window._allItems = new List<DropdownElement>(allItems);
            window._selectedItems = new HashSet<int>(currentSelection.Select(item => item.id));
            window._onClose = onClose;

            Rect activatorRect = activator.Button.worldBound;
            var screenRect = GUIUtility.GUIToScreenRect(activatorRect);
            float height = Mathf.Min(allItems.Count * ItemHeight, MaxHeight);
            float width = Mathf.Max(activatorRect.width, currentSelection.Max(item => EditorStyles.label.CalcSize(new GUIContent(item.name + "          ")).x)); 
            window.ShowAsDropDown(screenRect, new Vector2(width, height));
            window.IsOpen = true;
            
            return window;
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

            for (int i = 0; i < _allItems.Count; i++)
            {
                DropdownElement item = _allItems[i];
                bool selected = _selectedItems.Contains(item.id);
                bool newSelected = EditorGUILayout.ToggleLeft(item.name, selected);
                if (newSelected && !selected)
                    _selectedItems.Add(item.id);
                else if (!newSelected && selected)
                    _selectedItems.Remove(item.id);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnLostFocus()
        {
            _onClose?.Invoke(_allItems.Where(item => _selectedItems.Contains(item.id)).ToList());
            Close();
            IsOpen = false;
        }
    }
}