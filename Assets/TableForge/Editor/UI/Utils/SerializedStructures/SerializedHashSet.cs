using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TableForge.Editor.UI
{
    [Serializable]
    internal class SerializedHashSet<T> : ISerializationCallbackReceiver, ISet<T>, IReadOnlyCollection<T>
    {
        [SerializeField]
        private List<T> values = new();

        private HashSet<T> _hashSet = new();
        
        public IReadOnlyList<T> Values => _hashSet.ToList();

        #region Constructors

        // Parameterless constructor required for Unity serialization
        public SerializedHashSet() { }

        public SerializedHashSet(IEnumerable<T> collection)
        {
            _hashSet = new HashSet<T>(collection);
        }

        #endregion Constructors

        #region ISet<T> Implementation

        public int Count => _hashSet.Count;
        public bool IsReadOnly => false;

        void ICollection<T>.Add(T item) => _hashSet.Add(item);
        public bool Add(T item) => _hashSet.Add(item);
        public bool Remove(T item) => _hashSet.Remove(item);
        public void ExceptWith(IEnumerable<T> other) => _hashSet.ExceptWith(other);
        public void IntersectWith(IEnumerable<T> other) => _hashSet.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => _hashSet.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _hashSet.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _hashSet.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _hashSet.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _hashSet.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => _hashSet.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => _hashSet.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => _hashSet.UnionWith(other);
        public void Clear() => _hashSet.Clear();
        public bool Contains(T item) => _hashSet.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _hashSet.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion ISet<T> Implementation

        #region ISerializationCallbackReceiver Implementation

        public void OnBeforeSerialize()
        {
            values.Clear();
            values.AddRange(_hashSet);
        }

        public void OnAfterDeserialize()
        {
            _hashSet = new HashSet<T>();
            foreach (var val in values)
            {
                _hashSet.Add(val);
            }
        }

        #endregion ISerializationCallbackReceiver Implementation
    }
}
