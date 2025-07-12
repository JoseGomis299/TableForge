using System.Collections;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that contains a collection of items.
    /// </summary>
    internal interface ICollectionCell 
    {
        #region Public Methods
        
        /// <summary>
        /// Adds an item to the collection. Creating the corresponding row in the sub table.
        /// </summary>
        /// <param name="item">The item to add</param>
        void AddItem(object item);
        
        /// <summary>
        /// Adds an empty item to the collection. Creating the corresponding row in the sub table.
        /// </summary>
        void AddEmptyItem();
        
        /// <summary>
        /// Removes an item from the collection.
        /// <remarks>This method does not remove the corresponding row in the sub table. To do that see <see cref="Table.RemoveRow"/></remarks>
        /// </summary>
        /// <param name="position">The position of the row to remove.</param>
        void RemoveItem(int position);
        
        /// <summary>
        ///  Returns a copy of the collection items.
        /// </summary>
        ICollection GetItems();
        
        /// <summary>
        /// The number of items in the collection.
        /// </summary>
        int Count { get; }
        
        #endregion
    }
}