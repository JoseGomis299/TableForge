using System;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Factory class responsible for creating instances of different Cell types based on field type.
    /// </summary>
    internal static class CellFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a Cell instance appropriate for the given field type.
        /// </summary>
        /// <param name="column">The column in which this cell belongs.</param>
        /// <param name="row">The row in which this cell belongs.</param>
        /// <param name="fieldType">The type of the field to be serialized. (can be null, see 'Remarks' section)</param>
        /// <param name="fieldInfo">Metadata about the field.</param>
        /// <param name="tfSerializedObject">The serialized object containing the field.</param>
        /// <remarks>
        /// Is acceptable to receive a null fieldInfo parameter if the field is part of a collection.
        /// </remarks>
        /// <returns>A Cell instance matching the given type, or a default cell if no match is found.</returns>
        public static Cell CreateCell(CellAnchor column, Row row, Type fieldType, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject)
        {
            // Check for exact type matches first (e.g., `bool`, `string`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, fieldType, out var cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo, tfSerializedObject);

            // Check for assignable types (e.g., `IList`, `ICollection`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Assignable, fieldType, out cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo, tfSerializedObject);

            // Check for generic types (e.g., `List<T>`, `IList<T>`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.GenericArgument, fieldType, out cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo, tfSerializedObject);

            // Handle special cases
            if (fieldType.IsEnum)
                return new EnumCell(column, row, fieldInfo, tfSerializedObject);

            if (fieldType.IsSerializableClassOrStruct())
                return new SubitemCell(column, row, fieldInfo, tfSerializedObject);

            Debug.LogWarning($"Unsupported type: {fieldType}\nField: {fieldInfo?.Name}\nAt table: {column.Table.Name}\nPosition: {column.LetterPosition}{row.Position}");
            return new DefaultCell(column, row, fieldInfo, tfSerializedObject);
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Creates an instance of a specific Cell type using reflection.
        /// </summary>
        /// <param name="cellType">The type of cell to create.</param>
        /// <param name="column">The column in which this cell belongs.</param>
        /// <param name="row">The row in which this cell belongs.</param>
        /// <param name="fieldInfo">Metadata about the field.</param>
        /// <param name="tfSerializedObject">The serialized object containing the field.</param>
        /// <returns>An instance of the specified Cell type, or null if creation fails.</returns>
        private static Cell CreateCellInstance(Type cellType, CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject)
        {
            var constructor = cellType.GetConstructor(
                new[] { typeof(CellAnchor), typeof(Row), typeof(TFFieldInfo), typeof(ITFSerializedObject) }
            );

            if (constructor != null)
                return (Cell)constructor.Invoke(new object[] { column, row, fieldInfo, tfSerializedObject });

            Debug.LogError($"{cellType.Name} lacks required constructor.");
            return null;
        }

        #endregion

    }
}
