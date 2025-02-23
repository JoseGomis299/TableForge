using System;

namespace TableForge
{
    /// <summary>
    /// Represents an abstract cell within a table, storing and managing field values.
    /// </summary>
    internal abstract class Cell
    {
        #region Fields

        /// <summary>
        /// The column in which this cell belongs.
        /// </summary>
        public readonly CellAnchor Column;

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

        #endregion

        #region Constructors
        protected Cell(CellAnchor column, Row row, TFFieldInfo fieldInfo)
        {
            Column = column;
            Row = row;
            FieldInfo = fieldInfo;
            Type = GetFieldType();
            
            Value = GetFieldValue();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the current value stored in this cell.
        /// </summary>
        /// <returns>The object value of the cell.</returns>
        public virtual object GetValue()
        {
            return GetFieldValue();
        }

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
        public string GetPosition() => $"{Row.LetterPosition}{Column.Position}";

        /// <summary>
        /// Retrieves the current value of the field stored in this cell.
        /// </summary>
        /// <returns>The field value as an object.</returns>
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
        
        #region Protected Methods
        
        protected Type GetFieldType()
        {
            return TfSerializedObject.GetValueType(this);
        }
        
        #endregion
    }
}
