using System.Collections;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that contains a collection of items.
    /// </summary>
    internal abstract class CollectionCell : SubTableCell, ICollectionCell
    {
        public int Count => cachedValue is ICollection collection ? collection.Count : 0;

        protected CollectionCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
        }
        
        public abstract void AddItem(object item);
        public abstract void AddEmptyItem();
        public abstract void RemoveItem(int position);
        public abstract ICollection GetItems();
        
        public override int CompareTo(Cell other)
        {
            if (other is not CollectionCell collectionCell) return 1; 
            
            // Compare the number of items in the collections
            int thisCount = Count;
            int otherCount = collectionCell.Count;

            return thisCount.CompareTo(otherCount);
        }
        
    }
}