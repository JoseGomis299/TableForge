namespace TableForge.Editor
{
    internal interface ITfSwapableCollectionItem : ITfSerializedCollectionItem
    {
        #region Public Methods

        /// <summary>
        /// Swaps the item with another item in the collection.
        /// </summary>
        void SwapWith(ITfSwapableCollectionItem other);

        #endregion
    }
    
    internal interface ITfSwapableCollectionItem<T> : ITfSerializedCollectionItem where T : ITfSwapableCollectionItem
    {
        #region Public Methods

        /// <summary>
        /// Swaps the item with another item in the collection.
        /// </summary>
        void SwapWith(T other);

        #endregion
    }
}