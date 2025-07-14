using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge.Editor.Serialization
{
    internal abstract class SubTableCellSerializer : CellSerializer
    {
        protected SubTableCell SubTableCell => (SubTableCell)cell;
        protected SubTableCellSerializer(Cell cell) : base(cell)
        {
            if (cell is not Editor.SubTableCell)
            {
                throw new System.ArgumentException("Cell must be of type SubTableCell", nameof(cell));
            }
            
            serializer = null;
        }

        public override string Serialize()
        {
            if (SerializationConstants.subTablesAsJson || cell.GetAncestors().Any(x => x is ICollectionCell))
            {
                return SerializeAsJson();
            }
            
            return SerializeFlattening();
        }
        
        private string SerializeAsJson()
        {
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonObjectStart);

            IEnumerable<Cell> descendants = cell.GetImmediateDescendants();

            foreach (var descendant in descendants)
            {
                string value;
                if(descendant.Serializer is IQuotedValueCellSerializer quotedValueCell) value = quotedValueCell.SerializeQuotedValue(true);
                else value = descendant.Serializer.Serialize();
                serializedData.Append($"\"{descendant.column.Name}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}");
            }

            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1); 
            }

            serializedData.Append(SerializationConstants.JsonObjectEnd);
            return serializedData.ToString();
        }
        
        private string SerializeFlattening()
        {
            if (SubTableCell.SubTable.Rows.Count == 0)
            {
                string emptyTable = "";
                for (int i = 0; i < SubTableCell.GetSubTableColumnCount(); i++)
                {
                    emptyTable += SerializationConstants.EmptyColumn;
                    emptyTable += SerializationConstants.columnSeparator;
                }
                emptyTable += SerializationConstants.EmptyColumn;
                return emptyTable;
            }
            
            StringBuilder serializedData = new StringBuilder();
            foreach (var descendant in cell.GetImmediateDescendants())
            {
                serializedData.Append(SerializationConstants.csvCompatible ? 
                    descendant.SerializeCellCsvCompatible(true) 
                    : descendant.Serializer.Serialize())
                    .Append(SerializationConstants.columnSeparator);
            }
            
            // Remove the last column separator
            if (serializedData.Length > 0)
            {
                serializedData.Remove(serializedData.Length - SerializationConstants.columnSeparator.Length, SerializationConstants.columnSeparator.Length);
            }
            
            return serializedData.ToString();
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            int index = 0;
            if (TryDeserializeJson(data, ref index))
            {
                return; // Successfully deserialized as JSON
            }
            
            string[] values = data.Split(SerializationConstants.columnSeparator);
            DeserializeSubTable(values, ref index);
        }

        private bool TryDeserializeJson(string data, ref int index)
        {
            if (!data.StartsWith(SerializationConstants.JsonObjectStart) || !data.EndsWith(SerializationConstants.JsonObjectEnd))
                return false;

            Dictionary<string, string> jsonFields;
            try
            {
                jsonFields = JsonUtil.ToStringDictionary(data);
            }
            catch
            {
                return false; // Invalid JSON format
            }

            // Ensure all fields in the JSON exist as columns in the subtable
            var subTableColumns = SubTableCell.SubTable.OrderedColumns.Select(c => c.Name).ToList();
            if (!jsonFields.Keys.All(key => subTableColumns.Contains(key)))
            {
                return false; //JSON fields do not match subTable columns
            }

            // Order the values based on the column order in the subTable
            var values = subTableColumns.Select(column => jsonFields.GetValueOrDefault(column, SerializationConstants.EmptyColumn)).ToArray();
            
            if (SerializationConstants.modifySubTables) DeserializeModifyingSubTable(values, ref index);
            else DeserializeWithoutModifyingSubTable(values, ref index);
            return true;
        }
        
        public void DeserializeSubTable(string[]values, ref int index)
        {
            if(cell.GetValue() == null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                index += SubTableCell.SubTable.Columns.Count;
                return;
            }
            
            if(SerializationConstants.modifySubTables) DeserializeModifyingSubTable(values, ref index);
            else DeserializeWithoutModifyingSubTable(values, ref index);
        }

        protected abstract void DeserializeModifyingSubTable(string[] values, ref int index);
        protected abstract void DeserializeWithoutModifyingSubTable(string[] values, ref int index);
        
        protected static void DeserializeCell(string[] values, ref int index, Cell cell)
        {
            if(cell is Editor.SubTableCell and not ICollectionCell && cell.Serializer is SubTableCellSerializer subTableCellSerializer && !JsonUtil.IsValidJsonObject(values[index]))
            {
                subTableCellSerializer.DeserializeSubTable(values, ref index);
            }
            else
            {
                string value = values[index].Replace(SerializationConstants.EmptyColumn, string.Empty);
                cell.Serializer.Deserialize(value);
                index++;
            }
        }
    }
} 