using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Abstract base class that defines common behavior for managing header visibility.
    /// </summary>
    internal abstract class VisibilityManager<THeader> : IHeaderVisibilityNotifier where THeader : HeaderControl 
    {
        public event Action<HeaderControl, int> OnHeaderBecameVisible;
        public event Action<HeaderControl, int> OnHeaderBecameInvisible;
        
        protected Vector2 securityExtraSize = new Vector2(UiConstants.CellWidth * 2, UiConstants.CellHeight * 4);
        
        protected readonly ScrollView scrollView;
        protected readonly List<THeader> visibleHeaders = new List<THeader>();
        protected readonly Dictionary<THeader, HashSet<object>> lockedVisibleHeaders = new(); 
        protected float lastScrollValue;
        protected readonly TableControl tableControl;
        
        private readonly List<THeader> _orderedLockedHeaders = new List<THeader>();
        private readonly HashSet<THeader> _invisibleHeadersThisFrame = new HashSet<THeader>();
        private readonly HashSet<THeader> _visibleHeadersThisFrame = new HashSet<THeader>();
        
        
        public bool IsRefreshingVisibility { get; protected set; }
        public IReadOnlyList<THeader> CurrentVisibleHeaders => visibleHeaders;
        public IReadOnlyList<THeader> OrderedLockedHeaders => _orderedLockedHeaders;
        protected VisibilityManager(TableControl tableControl)
        {
            this.tableControl = tableControl;
            scrollView = tableControl.ScrollView;
            
            lastScrollValue = float.MinValue;
        }
        
        public bool IsHeaderVisibilityLocked(THeader header)
        {
            return lockedVisibleHeaders.ContainsKey(header);
        }
        
        public bool IsHeaderVisibilityLockedBy(THeader header, object keyOwner)
        {
            if (!lockedVisibleHeaders.TryGetValue(header, out var owners)) return false;
            
            return owners.Contains(keyOwner);
        }
        
        public void Clear()
        {
            foreach (var header in visibleHeaders)
            {
                header.IsVisible = false;
            }
            visibleHeaders.Clear();
            lockedVisibleHeaders.Clear();
            _orderedLockedHeaders.Clear();
            
            lastScrollValue = float.MinValue;
        }
        
        public void LockHeaderVisibility(THeader header, object keyOwner)
        {
            if (lockedVisibleHeaders.TryAdd(header, new HashSet<object>()))
            {
                _orderedLockedHeaders.Add(header);
                _orderedLockedHeaders.Sort((x, y) => x.CellAnchor.Position.CompareTo(y.CellAnchor.Position));
            }
           
            lockedVisibleHeaders[header].Add(keyOwner);
            
            if (!header.IsVisible) 
                MakeHeaderVisible(header, insertAtTop: false); 
        }
        
        public void UnlockHeaderVisibility(THeader header, object keyOwner)
        {
            if (!lockedVisibleHeaders.TryGetValue(header, out var owners)) return;
            
            if (owners.Remove(keyOwner) && owners.Count == 0)
            {
                lockedVisibleHeaders.Remove(header);
                _orderedLockedHeaders.Remove(header);
            }
        }
        
        /// <summary>
        /// Shows a header by marking it as visible, adding it to the list, and notifying listeners.
        /// </summary>
        protected void MakeHeaderVisible(THeader header, bool insertAtTop)
        {
            if (insertAtTop)
                visibleHeaders.Insert(0, header);
            else
                visibleHeaders.Add(header);

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
            return tableControl.Filterer.IsVisible(header.CellAnchor.GetRootAnchor().Id) && (lockedVisibleHeaders.ContainsKey(header) || IsHeaderInBounds(header, true));
        }

        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        public abstract bool IsHeaderInBounds(THeader header, bool addSecuritySize);

        /// <summary>
        /// Checks whether the given header is completely within the bounds of the ScrollView.
        /// </summary>
        /// <param name="header">The header to check.</param>
        /// <param name="addSecuritySize">If some security extra size should be added.</param>
        /// <param name="visibleBounds">Binary values representing the visible bounds 2^1 meaning right or top and 2^0 meaning left or bottom</param>
        /// <returns></returns>
        public abstract bool IsHeaderCompletelyInBounds(THeader header, bool addSecuritySize, out sbyte visibleBounds);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        public abstract void RefreshVisibility(float delta);
    }
}