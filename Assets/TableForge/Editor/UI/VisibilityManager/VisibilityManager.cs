using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    /// <summary>
    /// Abstract base class that defines common behavior for managing header visibility.
    /// </summary>
    internal abstract class VisibilityManager<THeader> where THeader : HeaderControl
    {
        public event Action<HeaderControl> OnHeaderBecameVisible;
        public event Action<HeaderControl> OnHeaderBecameInvisible;

        protected readonly ScrollView _scrollView;
        protected readonly List<THeader> _visibleHeaders = new List<THeader>();
        protected readonly HashSet<THeader> _lockedVisibleHeaders = new HashSet<THeader>(); 
        
        public IReadOnlyList<THeader> VisibleHeaders => _visibleHeaders;

        protected VisibilityManager(ScrollView scrollView)
        {
            _scrollView = scrollView;
        }
        
        public virtual void Clear()
        {
            foreach (var header in _visibleHeaders)
            {
                header.IsVisible = false;
                NotifyHeaderBecameInvisible(header);
            }
            _visibleHeaders.Clear();
            _lockedVisibleHeaders.Clear();
        }
        
        public void LockHeaderVisibility(THeader header)
        {
            _lockedVisibleHeaders.Add(header);
            if(!header.IsVisible)
                MakeHeaderVisible(header, false);
        }
        
        public void UnlockHeaderVisibility(THeader header)
        {
            _lockedVisibleHeaders.Remove(header);
        }
        
        /// <summary>
        /// Shows a header by marking it as visible, adding it to the list, and notifying listeners.
        /// </summary>
        protected void MakeHeaderVisible(THeader header, bool insertAtTop)
        {
            if (insertAtTop)
                _visibleHeaders.Insert(0, header);
            else
                _visibleHeaders.Add(header);

            bool wasVisible = header.IsVisible;
            header.IsVisible = true;
            if (!wasVisible)
                NotifyHeaderBecameVisible(header);
        }

        protected virtual void NotifyHeaderBecameVisible(THeader header)
        {
            OnHeaderBecameVisible?.Invoke(header);
        }

        protected virtual void NotifyHeaderBecameInvisible(THeader header)
        {
            OnHeaderBecameInvisible?.Invoke(header);
        }

        /// <summary>
        /// Checks whether the given header is visible within the bounds of the ScrollView.
        /// </summary>
        protected abstract bool IsHeaderVisible(THeader header);

        /// <summary>
        /// Refreshes the visibility based on the current scroll position.
        /// </summary>
        public abstract void RefreshVisibility(float value);
    }
}