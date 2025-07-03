using System;
using System.Collections.Generic;
using System.Text;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents an abstract cell within a table, storing and managing field values.
    /// </summary>
    internal abstract class Cell : ISerializableCell, IComparable<Cell>
    {
        #region Fields

        /// <summary>
        /// The column in which this cell belongs.
        /// </summary>
        public readonly Column column;

        /// <summary>
        /// The row in which this cell belongs.
        /// </summary>
        public readonly Row row;

        /// <summary>
        /// Metadata about the field associated with this cell.
        /// </summary>
        public readonly TfFieldInfo fieldInfo;

        /// <summary>
        /// The serialized object containing the field.
        /// </summary>
        public ITfSerializedObject TfSerializedObject => row.SerializedObject;

        /// <summary>
        /// The cached value of the cell.
        /// </summary>
        protected object cachedValue;

        /// <summary>
        /// The serializer used to serialize and deserialize the cell's data.
        /// </summary>
        protected ISerializer serializer;
        

        #endregion

        #region Properties

        /// <summary>
        /// The type of the field stored in this cell.
        /// </summary>
        public Type Type { get; protected set; }
        
        /// <summary>
        /// The table in which this cell belongs.
        /// </summary>
        public Table Table => column.Table;

        /// <summary>
        /// Unique identifier of the cell in the table.
        /// </summary>
        public int Id { get; }

        #endregion

        #region Constructors
        protected Cell(Column column, Row row, TfFieldInfo fieldInfo)
        {
            this.column = column;
            this.row = row;
            this.fieldInfo = fieldInfo;
            Type = GetFieldType();
            cachedValue = GetFieldValue();
            serializer = new JsonSerializer();
            
            Id = HashCodeUtil.CombineHashes(this.column.Id, this.row.Id, Type.Name, fieldInfo?.Name);
            this.RegisterCell();
        }
        
        #endregion

        #region Public Methods
        
        public object GetValue() => cachedValue;

        /// <summary>
        /// Sets the value of this cell and updates the serialized object.
        /// </summary>
        /// <param name="value">The new value to be set.</param>
        public virtual void SetValue(object value)
        {
            SetFieldValue(value);
            cachedValue = value;
        }

        /// <summary>
        /// Retrieves and stores the current value of the field stored in this cell.
        /// </summary>
        public virtual void RefreshData()
        {
            cachedValue = GetFieldValue();
        }
        
        /// <summary>
        /// Gets the position of the cell in the table in a spreadsheet like format.
        /// </summary>
        /// <example>
        /// If the cell is in the first row and the first column, the position would be "A1".
        /// </example>
        /// <returns>A string representing the cell's position .</returns>
        public string GetLocalPosition() => $"{column.LetterPosition}{row.Position}";
        
        
        /// <summary>
        ///  Gets the global position of the cell in the table, which is a concatenation of all ancestor cells' local positions.
        /// </summary>
        /// <example>
        /// If the cell is in the first row and the first column of a sub-table that is in the second row and second column, the position would be "B2.A1".
        /// </example>
        /// <returns>A string representing the global cell's position.</returns>
        public string GetGlobalPosition()
        {
            StringBuilder positionBuilder = new StringBuilder();
            foreach (var cell in this.GetAncestors(true))
            {
                positionBuilder.Insert(0, $".{cell.GetLocalPosition()}");
            }
            
            if (positionBuilder.Length > 0)
            {
                positionBuilder.Remove(0, 1); // Remove the leading dot
            }
            
            return positionBuilder.ToString();
        }
        
        public abstract string Serialize();
        public abstract void Deserialize(string data);
        public abstract int CompareTo(Cell other);
        public bool TryDeserialize(string data)
        {
            try
            {
                Deserialize(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        #endregion
        
        #region Protected Methods
        
        protected Type GetFieldType()
        {
            return TfSerializedObject.GetValueType(this);
        }
        
        /// <summary>
        /// Gets the current value stored in this cell.
        /// </summary>
        /// <returns>The object value of the cell.</returns>
        protected object GetFieldValue()
        {
            return TfSerializedObject.GetValue(this);
        }
        
        /// <summary>
        /// Sets the field value in the serialized object.
        /// </summary>
        /// <param name="value">The new value to be stored.</param>
        protected void SetFieldValue(object value)
        {
            TfSerializedObject.SetValue(this, value);
        }
        #endregion
    }
}
