using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge.Editor.Serialization
{
    internal class ListCellSerializer : CollectionCellSerializer
    {
        public ListCellSerializer(Cell cell) : base(cell)
        {
            if (cell is not ListCell)
                throw new System.ArgumentException("Cell must be of type ListCell", nameof(cell));
        }

        protected override string SerializeCollection(SerializationOptions options)
        {
            return SerializeList(options, cell.GetImmediateDescendants());
        }

        private string SerializeList(SerializationOptions options, IEnumerable<Cell> collection)
        {
            var cells = collection.ToList();
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonArrayStart);
            int currentRow = -1;
            bool isSimpleType = cells.FirstOrDefault()?.row.SerializedObject.SerializedType.Type.IsSimpleType() ?? false;
            foreach (var item in cells)
            {
                if (currentRow != item.row.Position)
                {
                    if (currentRow != -1)
                    {
                        serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                        serializedData.Append(isSimpleType ? SerializationConstants.JsonItemSeparator : $"{SerializationConstants.JsonObjectEnd}{SerializationConstants.JsonItemSeparator}");
                    }
                    currentRow = item.row.Position;
                    if (!isSimpleType && item.row.OrderedCells.Count > 1)
                        serializedData.Append("{");
                }
                string value;
                if (item.Serializer is IQuotedValueCellSerializer quotedValueCell) value = quotedValueCell.SerializeQuotedValue(options, true);
                else value = item.Serializer.Serialize(options);
                value = isSimpleType
                    ? $"{value}{SerializationConstants.JsonItemSeparator}"
                    : $"\"{item.column.Name}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}";
                serializedData.Append(value);
            }
            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1); // Remove trailing comma
                if (!isSimpleType && cells.Last().row.OrderedCells.Count > 1)
                    serializedData.Append(SerializationConstants.JsonObjectEnd);
            }
            serializedData.Append(SerializationConstants.JsonArrayEnd);
            return serializedData.ToString();
        }
    }
} 