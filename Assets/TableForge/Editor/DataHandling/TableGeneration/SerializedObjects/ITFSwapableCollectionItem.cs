namespace TableForge.Editor
{
    internal interface ITFSwapableCollectionItem : ITFSerializedCollectionItem
    {
        #region Public Methods

        /// <summary>
        /// Swaps the item with another item in the collection.
        /// </summary>
        void SwapWith(ITFSwapableCollectionItem other);

        #endregion
    }
    
    internal interface ITFSwapableCollectionItem<T> : ITFSerializedCollectionItem where T : ITFSwapableCollectionItem
    {
        #region Public Methods

        /// <summary>
        /// Swaps the item with another item in the collection.
        /// </summary>
        void SwapWith(T other);

        #endregion
    }
}