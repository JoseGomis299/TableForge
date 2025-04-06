using System;
using System.Collections.Generic;
using System.Linq;
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
        protected readonly HashSet<THeader> LockedVisibleHeaders = new HashSet<THeader>(); 
        protected float LastScrollValue;
        protected readonly TableControl TableControl;
        
        private readonly List<THeader> _orderedLockedHeaders = new List<THeader>();
        private readonly HashSet<THeader> _invisibleHeadersThisFrame = new HashSet<THeader>();
        private readonly HashSet<THeader> _visibleHeadersThisFrame = new HashSet<THeader>();
        
        
        public IReadOnlyList<THeader> CurrentVisibleHeaders => VisibleHeaders;
        public IReadOnlyList<THeader> OrderedLockedHeaders => _orderedLockedHeaders;
        protected VisibilityManager(TableControl tableControl, ScrollView scrollView)
        {
            TableControl = tableControl;
            ScrollView = scrollView;
            
            LastScrollValue = float.MinValue;
        }
        
        public bool IsHeaderVisibilityLocked(THeader header)
        {
            return LockedVisibleHeaders.Contains(header);
        }
        
        public void Clear()
        {
            foreach (var header in VisibleHeaders)
            {
                header.IsVisible = false;
            }
            VisibleHeaders.Clear();
            LockedVisibleHeaders.Clear();
            
            LastScrollValue = float.MinValue;
        }
        
        public void LockHeaderVisibility(THeader header)
        {
            LockedVisibleHeaders.Add(header);
            if(!header.IsVisible)
                MakeHeaderInvisible(header);
            
            _orderedLockedHeaders.Add(header);
            _orderedLockedHeaders.Sort((x, y) => x.CellAnchor.Position.CompareTo(y.CellAnchor.Position));
        }
        
        public void UnlockHeaderVisibility(THeader header)
        {
            LockedVisibleHeaders.Remove(header);
            _orderedLockedHeaders.Remove(header);
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
            return LockedVisibleHeaders.Contains(header) || IsHeaderInBounds(header);
        }

        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        public abstract bool IsHeaderInBounds(THeader header);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        public abstract void RefreshVisibility(float delta);
    }
}