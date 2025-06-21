using System.Collections;
using System.Collections.Generic;

namespace TableForge
{
    /// <summary>
    /// Represents a cell that contains a collection of items.
    /// </summary>
    internal abstract class CollectionCell : SubTableCell, ICollectionCell
    {
        public int Count => Value is ICollection collection ? collection.Count : 0;

        protected CollectionCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
        }
        
        public abstract void AddItem(object item);
        public abstract void AddEmptyItem();
        public abstract void RemoveItem(int position);
        public abstract ICollection GetItems();

        public override string Serialize()
        {
            return SerializeCollection();
        }

        protected abstract string SerializeCollection();
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            
            var values = JsonUtil.JsonArrayToStringList(data);
            if(values.Count == 0) return;
            
            int index = 0;
            DeserializeSubTable(values.ToArray(), ref index);
        }

        public override int CompareTo(Cell other)
        {
            if (other is not CollectionCell collectionCell) return 1; 
            
            // Compare the number of items in the collections
            int thisCount = Count;
            int otherCount = collectionCell.Count;

            return thisCount.CompareTo(otherCount);
        }

        protected override void DeserializeModifyingSubTable(string[] values, ref int index)
        {
            Row currentRow = null;
            Stack<int> positionsToRemove = new Stack<int>();
            foreach (var descendant in this.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(currentRow != descendant!.Row) 
                    {
                        currentRow = descendant.Row;
                        positionsToRemove.Push(descendant.Row.Position);
                    }
                    
                    continue;
                }
                
                currentRow = descendant.Row;
                DeserializeCell(values, ref index, descendant);
            }
            
            // Remove the items that are not in the new data
            foreach (var indexToRemove in positionsToRemove)
            {
                RemoveItem(indexToRemove);
            }
            
            // Add new items if there are any
            while(index < values.Length)
            {
                AddEmptyItem();
                int lastRow = SubTable.Rows.Count;
                
                foreach (var cell in  SubTable.Rows[lastRow].OrderedCells)
                {
                    if (index >= values.Length)
                    {
                       break;
                    }
                    
                    DeserializeCell(values, ref index, cell);
                }
            }
        }

        protected override void DeserializeWithoutModifyingSubTable(string[] values, ref int index)
        {
            foreach (var descendant in this.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(SerializationConstants.ModifySubTables)
                        break;
                    index = 0;
                }
                
                if(descendant is SubTableCell subTableCell and not ICollectionCell)
                {
                    subTableCell.DeserializeSubTable(values, ref index);
                }
                else
                {
                    string value = values[index].Replace(SerializationConstants.EmptyColumn, string.Empty);
                    descendant.Deserialize(value);
                    index++;
                }
            }
        }
        
        
    }
}