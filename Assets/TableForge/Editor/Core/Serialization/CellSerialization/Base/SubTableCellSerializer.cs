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

        public override string Serialize(SerializationOptions options)
        {
            if (options.SubTablesAsJson || cell.GetAncestors().Any(x => x is ICollectionCell))
            {
                return SerializeAsJson(options);
            }
            
            return SerializeFlattening(options);
        }
        
        private string SerializeAsJson(SerializationOptions options)
        {
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonObjectStart);

            IEnumerable<Cell> descendants = cell.GetImmediateDescendants();

            foreach (var descendant in descendants)
            {
                string value;
                if(descendant.Serializer is IQuotedValueCellSerializer quotedValueCell) value = quotedValueCell.SerializeQuotedValue(options, true);
                else value = descendant.Serializer.Serialize(options);
                serializedData.Append($"\"{descendant.column.Name}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}");
            }

            if (serializedData.Length > 1)
            {
                serializedData.Remove(serializedData.Length - 1, 1); 
            }

            serializedData.Append(SerializationConstants.JsonObjectEnd);
            return serializedData.ToString();
        }
        
        private string SerializeFlattening(SerializationOptions options)
        {
            if (SubTableCell.SubTable.Rows.Count == 0)
            {
                string emptyTable = "";
                for (int i = 0; i < SubTableCell.GetSubTableColumnCount(); i++)
                {
                    emptyTable += SerializationConstants.EmptyColumn;
                    emptyTable += options.ColumnSeparator;
                }
                emptyTable += SerializationConstants.EmptyColumn;
                return emptyTable;
            }
            
            StringBuilder serializedData = new StringBuilder();
            foreach (var descendant in cell.GetImmediateDescendants())
            {
                serializedData.Append(options.CsvCompatible ? 
                    descendant.SerializeCellCsvCompatible(options, true) 
                    : descendant.Serializer.Serialize(options))
                    .Append(options.ColumnSeparator);
            }
            
            // Remove the last column separator
            if (serializedData.Length > 0)
            {
                serializedData.Remove(serializedData.Length - options.ColumnSeparator.Length, options.ColumnSeparator.Length);
            }
            
            return serializedData.ToString();
        }
        
        public override void Deserialize(string data, SerializationOptions options)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            int index = 0;
            if (TryDeserializeJson(data, ref index, options))
            {
                return; // Successfully deserialized as JSON
            }
            
            string[] values = data.Split(options.ColumnSeparator);
            DeserializeSubTable(values, ref index, options);
        }

        private bool TryDeserializeJson(string data, ref int index, SerializationOptions options)
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
            
            if (options.ModifySubTables) DeserializeModifyingSubTable(values, ref index, options);
            else DeserializeWithoutModifyingSubTable(values, ref index, options);
            return true;
        }
        
        public void DeserializeSubTable(string[]values, ref int index, SerializationOptions options)
        {
            if(cell.GetValue() == null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                index += SubTableCell.SubTable.Columns.Count;
                return;
            }
            
            if(options.ModifySubTables) DeserializeModifyingSubTable(values, ref index, options);
            else DeserializeWithoutModifyingSubTable(values, ref index, options);
        }

        protected abstract void DeserializeModifyingSubTable(string[] values, ref int index, SerializationOptions options);
        protected abstract void DeserializeWithoutModifyingSubTable(string[] values, ref int index, SerializationOptions options);
        
        protected static void DeserializeCell(string[] values, ref int index, Cell cell, SerializationOptions options)
        {
            if(cell is Editor.SubTableCell and not ICollectionCell && cell.Serializer is SubTableCellSerializer subTableCellSerializer && !JsonUtil.IsValidJsonObject(values[index]))
            {
                subTableCellSerializer.DeserializeSubTable(values, ref index, options);
            }
            else
            {
                string value = values[index].Replace(SerializationConstants.EmptyColumn, string.Empty);
                cell.Serializer.Deserialize(value, options);
                index++;
            }
        }
    }
} 