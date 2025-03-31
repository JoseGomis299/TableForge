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
        
        public THeader FirstVisibleHeader {get; private set;}
        
        protected Vector2 SecurityExtraSize = new Vector2(UiConstants.CellWidth * 2, UiConstants.CellHeight * 4);
        
        protected readonly ScrollView ScrollView;
        protected readonly List<THeader> VisibleHeaders = new List<THeader>();
        protected readonly HashSet<THeader> LockedVisibleHeaders = new HashSet<THeader>(); 
        protected int LastDirection = 0;
        protected int StartingIndex = -1;
        protected int EndingIndex = -1;
        
        private readonly List<THeader> _invisibleHeadersThisFrame = new List<THeader>();
        private readonly List<THeader> _visibleHeadersThisFrame = new List<THeader>();
        
        
        public IReadOnlyList<THeader> CurrentVisibleHeaders => VisibleHeaders;

        protected VisibilityManager(ScrollView scrollView)
        {
            ScrollView = scrollView;
        }
        
        public virtual void Clear()
        {
            foreach (var header in VisibleHeaders)
            {
                header.IsVisible = false;
                NotifyHeaderBecameInvisible(header, 0);
            }
            VisibleHeaders.Clear();
            LockedVisibleHeaders.Clear();
        }
        
        public void LockHeaderVisibility(THeader header)
        {
            LockedVisibleHeaders.Add(header);
            if(!header.IsVisible)
                MakeHeaderVisible(header, false, 0);
        }
        
        public void UnlockHeaderVisibility(THeader header)
        {
            LockedVisibleHeaders.Remove(header);
        }
        
        /// <summary>
        /// Shows a header by marking it as visible, adding it to the list, and notifying listeners.
        /// </summary>
        protected void MakeHeaderVisible(THeader header, bool insertAtTop, int direction)
        {
            if (insertAtTop)
                VisibleHeaders.Insert(0, header);
            else
                VisibleHeaders.Add(header);

            bool wasVisible = header.IsVisible;
            header.IsVisible = true;
            
            int invisibleIndex = _invisibleHeadersThisFrame.IndexOf(header);
            if(invisibleIndex != -1) _invisibleHeadersThisFrame.RemoveAt(invisibleIndex);
            
            if(FirstVisibleHeader == null)
                FirstVisibleHeader = header;
            else
            {
                if(!FirstVisibleHeader.IsVisible)
                    FirstVisibleHeader = header;
                else if(FirstVisibleHeader.CellAnchor.Position > header.CellAnchor.Position)
                    FirstVisibleHeader = header;
            }
            
            if (!wasVisible)
                _visibleHeadersThisFrame.Add(header);
        }
        
        protected void MakeHeaderInvisible(THeader header)
        {
            _invisibleHeadersThisFrame.Add(header);
        }
        
        protected void SendVisibilityNotifications()
        {
            foreach (var header in _invisibleHeadersThisFrame)
            {
                NotifyHeaderBecameInvisible(header, LastDirection);
            }
            
            foreach (var header in _visibleHeadersThisFrame)
            {
                NotifyHeaderBecameVisible(header, LastDirection);
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

        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        protected abstract bool IsHeaderVisible(THeader header);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        public abstract void RefreshVisibility(float delta);
    }
}