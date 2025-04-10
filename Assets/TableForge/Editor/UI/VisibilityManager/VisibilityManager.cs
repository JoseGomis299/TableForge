using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    /// <summary>
    /// Abstract base class that defines common behavior for managing header visibility.
    /// </summary>
    internal abstract class VisibilityManager<THeader> : IHeaderVisibilityNotifier where THeader : HeaderControl 
    {
        public event Action<HeaderControl, int> OnHeaderBecameVisible;
        public event Action<HeaderControl, int> OnHeaderBecameInvisible;
        
        protected Vector2 SecurityExtraSize = new Vector2(UiConstants.CellWidth * 2, UiConstants.CellHeight * 4);
        
        protected readonly ScrollView ScrollView;
        protected readonly List<THeader> VisibleHeaders = new List<THeader>();
        protected readonly Dictionary<THeader, HashSet<object>> LockedVisibleHeaders = new(); 
        protected float LastScrollValue;
        protected readonly TableControl TableControl;
        
        private readonly List<THeader> _orderedLockedHeaders = new List<THeader>();
        private readonly HashSet<THeader> _invisibleHeadersThisFrame = new HashSet<THeader>();
        private readonly HashSet<THeader> _visibleHeadersThisFrame = new HashSet<THeader>();
        
        
        public IReadOnlyList<THeader> CurrentVisibleHeaders => VisibleHeaders;
        public IReadOnlyList<THeader> OrderedLockedHeaders => _orderedLockedHeaders;
        protected VisibilityManager(TableControl tableControl)
        {
            TableControl = tableControl;
            ScrollView = tableControl.ScrollView;
            
            LastScrollValue = float.MinValue;
        }
        
        public bool IsHeaderVisibilityLocked(THeader header)
        {
            return LockedVisibleHeaders.ContainsKey(header);
        }
        
        public void Clear()
        {
            foreach (var header in VisibleHeaders)
            {
                header.IsVisible = false;
            }
            VisibleHeaders.Clear();
            LockedVisibleHeaders.Clear();
            _orderedLockedHeaders.Clear();
            
            LastScrollValue = float.MinValue;
        }
        
        public void LockHeaderVisibility(THeader header, object keyOwner)
        {
            if (LockedVisibleHeaders.TryAdd(header, new HashSet<object>()))
            {
                _orderedLockedHeaders.Add(header);
                _orderedLockedHeaders.Sort((x, y) => x.CellAnchor.Position.CompareTo(y.CellAnchor.Position));
            }
           
            LockedVisibleHeaders[header].Add(keyOwner);
        }
        
        public void UnlockHeaderVisibility(THeader header, object keyOwner)
        {
            if (!LockedVisibleHeaders.TryGetValue(header, out var owners)) return;
            
            if (owners.Remove(keyOwner) && owners.Count == 0)
            {
                LockedVisibleHeaders.Remove(header);
                _orderedLockedHeaders.Remove(header);
            }
        }
        
        /// <summary>
        /// Shows a header by marking it as visible, adding it to the list, and notifying listeners.
        /// </summary>
        protected void MakeHeaderVisible(THeader header, bool insertAtTop)
        {
            if (insertAtTop)
                VisibleHeaders.Insert(0, header);
            else
                VisibleHeaders.Add(header);

            bool wasVisible = header.IsVisible;
            header.IsVisible = true;
            
            _invisibleHeadersThisFrame.Remove(header);
            
            if (!wasVisible)
                _visibleHeadersThisFrame.Add(header);
        }
        
        protected void MakeHeaderInvisible(THeader header)
        {
            _invisibleHeadersThisFrame.Add(header);
        }
        
        protected void SendVisibilityNotifications(int direction)
        {
            foreach (var header in _invisibleHeadersThisFrame)
            {
                NotifyHeaderBecameInvisible(header, direction);
            }
            
            foreach (var header in _visibleHeadersThisFrame)
            {
                NotifyHeaderBecameVisible(header, direction);
            }
            
            _visibleHeadersThisFrame.Clear();
            _invisibleHeadersThisFrame.Clear();
        }

        protected virtual void NotifyHeaderBecameVisible(THeader header, int direction)
        {
            OnHeaderBecameVisible?.Invoke(header, direction);
        }

        protected virtual void NotifyHeaderBecameInvisible(THeader header, int direction)
        {
            OnHeaderBecameInvisible?.Invoke(header, direction);
        }
        
        public bool IsHeaderVisible(THeader header)
        {
            return LockedVisibleHeaders.ContainsKey(header) || IsHeaderInBounds(header, true);
        }

        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        public abstract bool IsHeaderInBounds(THeader header, bool addSecuritySize);

        public abstract bool IsHeaderCompletelyInBounds(THeader header, bool addSecuritySize, out float delta);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        public abstract void RefreshVisibility(float delta);
    }
}