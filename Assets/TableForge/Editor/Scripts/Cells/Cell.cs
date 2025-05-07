using System;

namespace TableForge
{
    /// <summary>
    /// Represents an abstract cell within a table, storing and managing field values.
    /// </summary>
    internal abstract class Cell : ISerializableCell
    {
        #region Fields

        /// <summary>
        /// The column in which this cell belongs.
        /// </summary>
        public readonly Column Column;

        /// <summary>
        /// The row in which this cell belongs.
        /// </summary>
        public readonly Row Row;

        /// <summary>
        /// Metadata about the field associated with this cell.
        /// </summary>
        public readonly TFFieldInfo FieldInfo;

        /// <summary>
        /// The serialized object containing the field.
        /// </summary>
        public ITFSerializedObject TfSerializedObject => Row.SerializedObject;

        /// <summary>
        /// The cached value of the cell.
        /// </summary>
        protected object Value;

        /// <summary>
        /// The serializer used to serialize and deserialize the cell's data.
        /// </summary>
        protected ISerializer Serializer;

        #endregion

        #region Properties

        /// <summary>
        /// The type of the field stored in this cell.
        /// </summary>
        public Type Type { get; protected set; }
        
        /// <summary>
        /// The table in which this cell belongs.
        /// </summary>
        public Table Table => Column.Table;

        /// <summary>
        /// Unique identifier of the cell in the table.
        /// </summary>
        public int Id { get; }

        #endregion

        #region Constructors
        protected Cell(Column column, Row row, TFFieldInfo fieldInfo)
        {
            Column = column;
            Row = row;
            FieldInfo = fieldInfo;
            Type = GetFieldType();
            Value = GetFieldValue();
            Serializer = new JsonSerializer();
            
            Id = HashCodeUtil.CombineHashes(Column.Id, Row.Id, GetPosition());
        }
        #endregion

        #region Public Methods
        
        public object GetValue() => Value;

        /// <summary>
        /// Sets the value of this cell and updates the serialized object.
        /// </summary>
        /// <param name="value">The new value to be set.</param>
        public virtual void SetValue(object value)
        {
            SetFieldValue(value);
            Value = value;
        }

        /// <summary>
        /// Retrieves and stores the current value of the field stored in this cell.
        /// </summary>
        public virtual void RefreshData()
        {
            Value = GetFieldValue();
        }
        
        /// <summary>
        /// Gets the position of the cell in the table in a spreadsheet like format.
        /// </summary>
        /// <example>
        /// If the cell is in the first row and the first column, the position would be "A1".
        /// </example>
        /// <returns>A string representing the cell's position .</returns>
        public string GetPosition() => $"{Column.LetterPosition}{Row.Position}";
        
        public abstract string Serialize();
        public abstract void Deserialize(string data);
        
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
