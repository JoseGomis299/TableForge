using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a cell that contains a subtable, typically used for complex or collection-based fields.
    /// </summary>
    internal abstract class SubTableCell : Cell
    {
        #region Properties

        /// <summary>
        /// The subtable associated with this cell.
        /// </summary>
        public Table SubTable { get; protected set; }

        #endregion

        #region Constructors

        protected SubTableCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            serializer = null;
        }

        #endregion
        
        #region Public Methods

        public override void RefreshData()
        {
            object value = cachedValue;
            base.RefreshData();
            
            if (value != cachedValue)
                CreateSubTable();
        }
        
        public override string Serialize()
        {
            if (SerializationConstants.subTablesAsJson || this.GetAncestors().Any(x => x is ICollectionCell))
            {
                return SerializeAsJson();
            }
            
            return SerializeFlattening();
        }
        
        private string SerializeAsJson()
        {
            StringBuilder serializedData = new StringBuilder(SerializationConstants.JsonObjectStart);

            IEnumerable<Cell> descendants = this.GetImmediateDescendants();

            foreach (var cell in descendants)
            {
                string value;
                if(cell is IQuotedValueCell quotedValueCell) value = quotedValueCell.SerializeQuotedValue(true);
                else value = cell.Serialize();
                serializedData.Append($"\"{cell.column.Name}\"{SerializationConstants.JsonKeyValueSeparator}{value}{SerializationConstants.JsonItemSeparator}");
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
            if (SubTable.Rows.Count == 0)
            {
                string emptyTable = "";
                for (int i = 0; i < this.GetSubTableColumnCount(); i++)
                {
                    emptyTable += SerializationConstants.EmptyColumn;
                    emptyTable += SerializationConstants.columnSeparator;
                }
                emptyTable += SerializationConstants.EmptyColumn;
                return emptyTable;
            }
            
            StringBuilder serializedData = new StringBuilder();
            foreach (var descendant in this.GetImmediateDescendants())
            {
                serializedData.Append(SerializationConstants.csvCompatible ? 
                    descendant.SerializeCellCsvCompatible(true) 
                    : descendant.Serialize())
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
            var subTableColumns = SubTable.OrderedColumns.Select(c => c.Name).ToList();
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
            if(cachedValue == null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                index += SubTable.Columns.Count;
                return;
            }
            
            if(SerializationConstants.modifySubTables) DeserializeModifyingSubTable(values, ref index);
            else DeserializeWithoutModifyingSubTable(values, ref index);
        }

        protected abstract void DeserializeModifyingSubTable(string[] values, ref int index);
        protected abstract void DeserializeWithoutModifyingSubTable(string[] values, ref int index);
        
        protected static void DeserializeCell(string[] values, ref int index, Cell cell)
        {
            if(cell is SubTableCell subTableCell and not ICollectionCell && !JsonUtil.IsValidJsonObject(values[index]))
            {
                subTableCell.DeserializeSubTable(values, ref index);
            }
            else
            {
                string value = values[index].Replace(SerializationConstants.EmptyColumn, string.Empty);
                cell.Deserialize(value);
                index++;
            }
        }
        
        #endregion
        
        #region Protected Methods

        /// <summary>
        /// Creates and initializes the subtable associated with this cell.
        /// </summary>
        protected abstract void CreateSubTable();
        
        #endregion
    }
}