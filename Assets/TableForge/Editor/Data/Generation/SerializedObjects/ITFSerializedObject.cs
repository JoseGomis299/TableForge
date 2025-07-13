using System;
using System.Collections.Generic;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents a serialized object within TableForge, providing methods for data access and manipulation.
    /// </summary>
    internal interface ITfSerializedObject
    {
        #region Properties

        string Name { get; }
        
        /// <summary>
        /// The root object from which the serialized object hierarchy originates.
        /// </summary>
        UnityEngine.Object RootObject { get; }
        
        /// <summary>
        /// The GUID of the root object.
        /// </summary>
        string RootObjectGuid { get; }
        
        /// <summary>
        /// The type of the serialized object.
        /// </summary>
        TfSerializedType SerializedType { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the value stored in a given cell.
        /// </summary>
        /// <param name="cell">The cell from which to retrieve the value.</param>
        /// <returns>The value of the cell.</returns>
        object GetValue(Cell cell);

        /// <summary>
        /// Sets a new value in the specified cell.
        /// </summary>
        /// <param name="cell">The cell to update.</param>
        /// <param name="data">The new value to be assigned.</param>
        void SetValue(Cell cell, object data);

        /// <summary>
        /// Retrieves the field type of the value stored in a given cell.
        /// </summary>
        /// <param name="cell">The cell for which to determine the type.</param>
        /// <returns>The type of the cell's value.</returns>
        Type GetValueType(Cell cell);

        /// <summary>
        /// Populates a row with cells and columns based on the serialized object's structure.
        /// </summary>
        /// <param name="columns">The list where columns will be stored.</param>
        /// <param name="table">The table that contains the row.</param>
        /// <param name="row">The row to be populated.</param>
        void PopulateRow(List<Column> columns, Table table, Row row);

        #endregion
    }
}