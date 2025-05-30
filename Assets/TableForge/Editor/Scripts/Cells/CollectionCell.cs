using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TableForge
{
    /// <summary>
    /// Represents a cell that contains a collection of items.
    /// </summary>
    internal abstract class CollectionCell : SubTableCell, ICollectionCell
    {
        protected CollectionCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
        }
        
        public abstract void AddItem(object item);
        public abstract void AddEmptyItem();
        public abstract void RemoveItem(int position);
        
        public abstract ICollection GetItems();

        public override string Serialize()
        {
            if (SubTable.Rows.Count == 0)
            {
                return SerializationConstants.CollectionItemStart + SerializationConstants.CollectionItemEnd;
            }
            
            return SerializeSubTable();
        }

        protected virtual string SerializeSubTable()
        {
            return SerializeCollection(this.GetImmediateDescendants());
        }

        protected string SerializeCollection(IEnumerable<Cell> collection)
        {
            StringBuilder serializedData = new StringBuilder();
            int currentRow = 0;
            foreach (var descendant in collection)
            {
                if(currentRow != descendant.Row.Position) 
                {
                    //Remove the last separator
                    if (currentRow != 0)
                    {
                        serializedData.Remove(serializedData.Length - SerializationConstants.CollectionSubItemSeparator.Length, SerializationConstants.CollectionSubItemSeparator.Length);
                        serializedData
                            .Append(SerializationConstants.CollectionItemEnd)
                            .Append(SerializationConstants.CollectionItemSeparator);
                    }
                    
                    serializedData
                        .Append(SerializationConstants.CollectionItemStart);
                    
                    currentRow = descendant.Row.Position;
                }
                
                serializedData
                    .Append(descendant.Serialize().Replace(SerializationConstants.ColumnSeparator, SerializationConstants.CollectionSubItemSeparator))
                    .Append(SerializationConstants.CollectionSubItemSeparator);
            }
            
            // Remove the last subitem start
            if (serializedData.Length > 0)
            {
                serializedData.Remove(serializedData.Length - SerializationConstants.CollectionSubItemSeparator.Length, SerializationConstants.CollectionSubItemSeparator.Length);
            }
            
            // Add the end of the collection
            serializedData.Append(SerializationConstants.CollectionItemEnd);
            
            return serializedData.ToString();
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            int index = 0;
            List<string> values = new List<string>();
            List<string> rows = data.SplitByLevel(0, SerializationConstants.CollectionItemStart, SerializationConstants.CollectionItemEnd);
            foreach (var row in rows)
            {
                values.AddRange(row.Split(SerializationConstants.CollectionSubItemSeparator));
            }
            
            DeserializeSubTable(values.ToArray(), ref index);
        }

        protected override void DeserializeModifying(string[] values, ref int index)
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

        protected override void DeserializeWithoutModifying(string[] values, ref int index)
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