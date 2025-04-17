using System.Text;

namespace TableForge
{
    /// <summary>
    /// Represents a cell that contains a subtable, typically used for complex or collection-based fields.
    /// </summary>
    internal abstract class SubTableCell : Cell
    {
        #region Properties

        /// <summary>
        /// Indicates whether the subtable is in an invalid state. This happens when the collection has been modified outside TableForge.
        /// </summary>
        public bool IsSubTableInvalid { get; set; }

        /// <summary>
        /// The subtable associated with this cell.
        /// </summary>
        public Table SubTable { get; protected set; }

        #endregion

        #region Constructors
        protected SubTableCell(Column column, Row row, TFFieldInfo fieldInfo)
            : base(column, row, fieldInfo) { }

        #endregion
        
        #region Public Methods

        public override void RefreshData()
        {
            object value = Value;
            base.RefreshData();
            
            if (value != Value)
                CreateSubTable();
        }
        
        public override string Serialize()
        {
            if (SubTable.Rows.Count == 0)
            {
                string emptyTable = "";
                for (int i = 0; i < this.GetSubTableColumnCount(); i++)
                {
                    emptyTable += SerializationConstants.EmptyColumn;
                    emptyTable += SerializationConstants.ColumnSeparator;
                }
                emptyTable += SerializationConstants.EmptyColumn;
                return emptyTable;
            }
            
            StringBuilder serializedData = new StringBuilder();
            foreach (var descendant in this.GetImmediateDescendants())
            {
                serializedData.Append(descendant.Serialize()).Append(SerializationConstants.ColumnSeparator);
            }
            
            // Remove the last column separator
            if (serializedData.Length > 0)
            {
                serializedData.Remove(serializedData.Length - SerializationConstants.ColumnSeparator.Length, SerializationConstants.ColumnSeparator.Length);
            }
            
            return serializedData.ToString();
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            string[] values = data.Split(SerializationConstants.ColumnSeparator);
            int index = 0;
            
            DeserializeSubTable(values, ref index);
        }

        public void DeserializeSubTable(string[]values, ref int index)
        {
            if(Value == null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                index += SubTable.Columns.Count;
                return;
            }
            
            if(SerializationConstants.ModifySubTables) DeserializeModifying(values, ref index);
            else DeserializeWithoutModifying(values, ref index);
        }

        protected abstract void DeserializeModifying(string[] values, ref int index);
        protected abstract void DeserializeWithoutModifying(string[] values, ref int index);
        
        protected static void DeserializeCell(string[] values, ref int index, Cell cell)
        {
            if(cell is SubTableCell subTableCell and not ICollectionCell)
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