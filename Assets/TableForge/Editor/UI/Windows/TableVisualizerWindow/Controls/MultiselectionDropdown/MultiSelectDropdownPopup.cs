using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace TableForge.UI
{
    internal class MultiSelectDropdownPopup : EditorWindow
    {
        private List<DropdownElement> _allItems;
        private HashSet<int> _selectedItems;
        private Action<List<DropdownElement>> _onClose;
        private Vector2 _scrollPos;

        private const float ItemHeight = 18f;
        private const float MaxHeight = 200f;

        public static void Show(List<DropdownElement> allItems, List<DropdownElement> currentSelection, Rect activatorRect, Action<List<DropdownElement>> onClose)
        {
            var window = CreateInstance<MultiSelectDropdownPopup>();
            window._allItems = new List<DropdownElement>(allItems);
            window._selectedItems = new HashSet<int>(currentSelection.Select(item => item.Id));
            window._onClose = onClose;

            float height = Mathf.Min(allItems.Count * ItemHeight, MaxHeight);
            var screenRect = GUIUtility.GUIToScreenRect(activatorRect);
            window.ShowAsDropDown(screenRect, new Vector2(screenRect.width, height));
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

            for (int i = 0; i < _allItems.Count; i++)
            {
                DropdownElement item = _allItems[i];
                bool selected = _selectedItems.Contains(item.Id);
                bool newSelected = EditorGUILayout.ToggleLeft(item.Name, selected);
                if (newSelected && !selected)
                    _selectedItems.Add(item.Id);
                else if (!newSelected && selected)
                    _selectedItems.Remove(item.Id);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnLostFocus()
        {
            _onClose?.Invoke(_allItems.Where(item => _selectedItems.Contains(item.Id)).ToList());
            Close();
        }
    }
}