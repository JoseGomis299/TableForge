using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Abstract base class that defines common behavior for managing header visibility.
    /// Handles virtual scrolling, visibility locking, and performance optimization for large tables.
    /// </summary>
    internal abstract class VisibilityManager<THeader> : IHeaderVisibilityNotifier where THeader : HeaderControl 
    {
        #region Events

        /// <summary>
        /// Event fired when a header becomes visible.
        /// </summary>
        public event Action<HeaderControl, int> OnHeaderBecameVisible;
        
        /// <summary>
        /// Event fired when a header becomes invisible.
        /// </summary>
        public event Action<HeaderControl, int> OnHeaderBecameInvisible;

        #endregion

        #region Protected Fields

        /// <summary>
        /// Extra size buffer for security when calculating visibility bounds.
        /// </summary>
        protected Vector2 securityExtraSize = new(UiConstants.CellWidth * 2, UiConstants.CellHeight * 4);
    
        /// <summary>
        /// Last scroll value for detecting scroll changes.
        /// </summary>
        protected float lastScrollValue;
        
        protected readonly ScrollView scrollView;
        protected readonly List<THeader> visibleHeaders = new();
        protected readonly Dictionary<THeader, HashSet<object>> lockedVisibleHeaders = new(); 
        protected readonly TableControl tableControl;

        #endregion

        #region Private Fields

        /// <summary>
        /// Ordered list of headers with locked visibility.
        /// </summary>
        private readonly List<THeader> _orderedLockedHeaders = new();
        
        /// <summary>
        /// Headers that became invisible in the current frame.
        /// </summary>
        private readonly HashSet<THeader> _invisibleHeadersThisFrame = new();
        
        /// <summary>
        /// Headers that became visible in the current frame.
        /// </summary>
        private readonly HashSet<THeader> _visibleHeadersThisFrame = new();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether the visibility manager is currently refreshing visibility.
        /// </summary>
        public bool IsRefreshingVisibility { get; protected set; }
        
        /// <summary>
        /// Gets the list of currently visible headers.
        /// </summary>
        public IReadOnlyList<THeader> CurrentVisibleHeaders => visibleHeaders;
        
        /// <summary>
        /// Gets the ordered list of headers with locked visibility.
        /// </summary>
        public IReadOnlyList<THeader> OrderedLockedHeaders => _orderedLockedHeaders;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the VisibilityManager class.
        /// </summary>
        /// <param name="tableControl">The table control to manage visibility for.</param>
        protected VisibilityManager(TableControl tableControl)
        {
            this.tableControl = tableControl;
            scrollView = tableControl.ScrollView;
            
            lastScrollValue = float.MinValue;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Subscribes to events that trigger visibility refresh.
        /// </summary>
        public abstract void SubscribeToRefreshEvents();
        
        /// <summary>
        /// Unsubscribes from events that trigger visibility refresh.
        /// </summary>
        public abstract void UnsubscribeFromRefreshEvents();
        
        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        /// <param name="header">The header to check.</param>
        /// <param name="addSecuritySize">Whether to add security extra size to the bounds.</param>
        /// <returns>True if the header is in bounds, false otherwise.</returns>
        public abstract bool IsHeaderInBounds(THeader header, bool addSecuritySize);

        /// <summary>
        /// Checks whether the given header is completely within the bounds of the ScrollView.
        /// </summary>
        /// <param name="header">The header to check.</param>
        /// <param name="addSecuritySize">Whether to add security extra size to the bounds.</param>
        /// <param name="visibleBounds">Binary values representing the visible bounds 2^1 meaning right or top and 2^0 meaning left or bottom.</param>
        /// <returns>True if the header is completely in bounds, false otherwise.</returns>
        public abstract bool IsHeaderCompletelyInBounds(THeader header, bool addSecuritySize, out sbyte visibleBounds);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        /// <param name="delta">The scroll delta since last refresh.</param>
        public abstract void RefreshVisibility(float delta);

        #endregion

        #region Public Methods - Visibility Locking

        public bool IsHeaderVisibilityLocked(THeader header)
        {
            return lockedVisibleHeaders.ContainsKey(header);
        }
        
        public bool IsHeaderVisibilityLockedBy(THeader header, object keyOwner)
        {
            if (!lockedVisibleHeaders.TryGetValue(header, out var owners)) return false;
            
            return owners.Contains(keyOwner);
        }
        
        /// <summary>
        /// Locks a header's visibility, ensuring it remains visible.
        /// </summary>
        /// <param name="header">The header to lock.</param>
        /// <param name="keyOwner">The object requesting the lock.</param>
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
        
        /// <summary>
        /// Unlocks a header's visibility for a specific object.
        /// </summary>
        /// <param name="header">The header to unlock.</param>
        /// <param name="keyOwner">The object releasing the lock.</param>
        public void UnlockHeaderVisibility(THeader header, object keyOwner)
        {
            if (!lockedVisibleHeaders.TryGetValue(header, out var owners)) return;
            
            if (owners.Remove(keyOwner) && owners.Count == 0)
            {
                lockedVisibleHeaders.Remove(header);
                _orderedLockedHeaders.Remove(header);
            }
        }

        #endregion

        #region Public Methods - State Management

        /// <summary>
        /// Clears all visibility state and resets the manager.
        /// </summary>
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

        public bool IsHeaderVisible(THeader header)
        {
            return tableControl.Filterer.IsVisible(header.CellAnchor.GetRootAnchor().Id) && 
                   (lockedVisibleHeaders.ContainsKey(header) || IsHeaderInBounds(header, true));
        }

        #endregion

        #region Protected Methods - Visibility Management

        /// <summary>
        /// Shows a header by marking it as visible, adding it to the list, and notifying listeners.
        /// </summary>
        /// <param name="header">The header to make visible.</param>
        /// <param name="insertAtTop">Whether to insert the header at the top of the visible list.</param>
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
        
        /// <summary>
        /// Marks a header as invisible for the current frame.
        /// </summary>
        /// <param name="header">The header to mark as invisible.</param>
        protected void MakeHeaderInvisible(THeader header)
        {
            _invisibleHeadersThisFrame.Add(header);
        }
        
        /// <summary>
        /// Sends visibility change notifications for headers that changed state this frame.
        /// </summary>
        /// <param name="direction">The direction of the visibility change.</param>
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

        #endregion

        #region Protected Methods - Event Notification

        /// <summary>
        /// Notifies listeners that a header became visible.
        /// </summary>
        /// <param name="header">The header that became visible.</param>
        /// <param name="direction">The direction of the visibility change.</param>
        protected void NotifyHeaderBecameVisible(THeader header, int direction)
        {
            OnHeaderBecameVisible?.Invoke(header, direction);
        }

        /// <summary>
        /// Notifies listeners that a header became invisible.
        /// </summary>
        /// <param name="header">The header that became invisible.</param>
        /// <param name="direction">The direction of the visibility change.</param>
        protected void NotifyHeaderBecameInvisible(THeader header, int direction)
        {
            OnHeaderBecameInvisible?.Invoke(header, direction);
        }

        #endregion
    }
}