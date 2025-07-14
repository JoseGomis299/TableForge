using System.Collections.Generic;

namespace TableForge.Editor.Serialization
{
    internal abstract class CollectionCellSerializer : SubTableCellSerializer
    {
        private CollectionCell CollectionCell => (CollectionCell)cell;
        protected CollectionCellSerializer(Cell cell) : base(cell)
        {
            if (cell is not TableForge.Editor.CollectionCell)
            {
                throw new System.ArgumentException("Cell must be of type CollectionCell", nameof(cell));
            }
        }

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
        
        protected override void DeserializeModifyingSubTable(string[] values, ref int index)
        {
            Row currentRow = null;
            Stack<int> positionsToRemove = new Stack<int>();
            foreach (var descendant in cell.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(currentRow != descendant!.row) 
                    {
                        currentRow = descendant.row;
                        positionsToRemove.Push(descendant.row.Position);
                    }
                    
                    continue;
                }
                
                currentRow = descendant.row;
                DeserializeCell(values, ref index, descendant);
            }
            
            // Remove the items that are not in the new data
            foreach (var indexToRemove in positionsToRemove)
            {
                CollectionCell.RemoveItem(indexToRemove);
            }
            
            // Add new items if there are any
            while(index < values.Length)
            {
                CollectionCell.AddEmptyItem();
                int lastRow = CollectionCell.SubTable.Rows.Count;
                
                foreach (var c in  CollectionCell.SubTable.Rows[lastRow].OrderedCells)
                {
                    if (index >= values.Length)
                    {
                       break;
                    }
                    
                    DeserializeCell(values, ref index, c);
                }
            }
        }

        protected override void DeserializeWithoutModifyingSubTable(string[] values, ref int index)
        {
            foreach (var descendant in cell.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(SerializationConstants.modifySubTables)
                        break;
                    index = 0;
                }
                
                if(descendant is SubTableCell _ and not ICollectionCell && descendant.Serializer is SubTableCellSerializer subTableCellSerializer)
                {
                    subTableCellSerializer.DeserializeSubTable(values, ref index);
                }
                else
                {
                    string value = values[index].Replace(SerializationConstants.EmptyColumn, string.Empty);
                    descendant.Serializer.Deserialize(value);
                    index++;
                }
            }
        }
    }
} 