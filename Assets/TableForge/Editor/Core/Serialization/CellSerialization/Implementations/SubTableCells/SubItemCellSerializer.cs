using System;
using System.Linq;

namespace TableForge.Editor.Serialization
{
    internal class SubItemCellSerializer : SubTableCellSerializer
    {
        private SubItemCell SubItemCell => (SubItemCell)cell;
        public SubItemCellSerializer(Cell cell) : base(cell)
        {
            if (cell is not SubItemCell _)
            {
                throw new ArgumentException("Cell must be of type SubItemCell", nameof(cell));
            }
        }

        protected override void DeserializeModifyingSubTable(string[]values, ref int index, SerializationOptions options)
        {
            if(cell.GetValue() != null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                cell.SetValue(null);
                return;
            }
            
            if(cell.GetValue() == null && !values[0].Equals(SerializationConstants.EmptyColumn))
            {
                SubItemCell.CreateDefaultValue();
            }
            
            DeserializeSubItem(values, ref index, options);
        }

        protected override void DeserializeWithoutModifyingSubTable(string[]values, ref int index, SerializationOptions options)
        {
            DeserializeSubItem(values, ref index, options);
        }

        private void DeserializeSubItem(string[] values, ref int index, SerializationOptions options)
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