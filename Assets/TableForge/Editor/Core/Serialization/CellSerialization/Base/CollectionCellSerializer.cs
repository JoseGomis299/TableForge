using System.Collections.Generic;
using System.Linq;
using TableForge.Editor.Serialization;

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

        public override string Serialize(SerializationOptions options)
        {
            return SerializeCollection(options);
        }

        protected abstract string SerializeCollection(SerializationOptions options);

        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            var values = JsonUtil.JsonArrayToStringList(data);
            if(values.Count == 0) return;
            
            if (JsonUtil.IsValidJsonObject(values[0]))
                values = values.SelectMany(v => JsonUtil.ToStringDictionary(v).Values).ToList();    
            
            int index = 0;
            DeserializeSubTable(values.ToArray(), ref index, options);
        }
        
        protected override void DeserializeModifyingSubTable(string[] values, ref int index, SerializationOptions options)
        {
            Row currentRow = null;
            Stack<int> positionsToRemove = new Stack<int>();
            foreach (var descendant in cell.GetImmediateDescendants().ToList())
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
                DeserializeCell(values, ref index, descendant, options);
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
                    
                    DeserializeCell(values, ref index, c, options);
                }
            }
        }

        protected override void DeserializeWithoutModifyingSubTable(string[] values, ref int index, SerializationOptions options)
        {
            foreach (var descendant in cell.GetImmediateDescendants().ToList())
            {
                if (index >= values.Length)
                {
                    if(options.ModifySubTables)
                        break;
                    index = 0;
                }
                
                DeserializeCell(values, ref index, descendant, options);
            }
        }
    }
} 