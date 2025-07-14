using System;

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

        protected override void DeserializeModifyingSubTable(string[]values, ref int index)
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
            
            DeserializeSubItem(values, ref index);
        }

        protected override void DeserializeWithoutModifyingSubTable(string[]values, ref int index)
        {
            DeserializeSubItem(values, ref index);
        }

        private void DeserializeSubItem(string[] values, ref int index)
        {
            foreach (var descendant in cell.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(SerializationConstants.modifySubTables)
                        break;
                    index = 0;
                }
                
                DeserializeCell(values, ref index, descendant);
            }
        }
    }
}