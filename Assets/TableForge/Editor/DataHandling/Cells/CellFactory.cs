using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Factory class responsible for creating instances of different Cell types based on field type.
    /// </summary>
    internal static class CellFactory
    {
        #region Fields

        private static readonly Dictionary<Type, ConstructorInfo> _cellConstructors = new();

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Creates a Cell instance appropriate for the given field type.
        /// </summary>
        /// <param name="column">The column in which this cell belongs.</param>
        /// <param name="row">The row in which this cell belongs.</param>
        /// <param name="fieldType">The type of the field to be serialized. (can be null, see 'Remarks' section)</param>
        /// <param name="fieldInfo">Metadata about the field.</param>
        /// <remarks>
        /// Is acceptable to receive a null fieldInfo parameter if the field is part of a collection.
        /// </remarks>
        /// <returns>A Cell instance matching the given type, or a default cell if no match is found.</returns>
        public static Cell CreateCell(Column column, Row row, Type fieldType, TfFieldInfo fieldInfo = null)
        {
            // Check for exact type matches first (e.g., `bool`, `string`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Exact, fieldType, out var cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo);

            // Check for assignable types (e.g., `IList`, `ICollection`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.Assignable, fieldType, out cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo);

            // Check for generic types (e.g., `List<T>`, `IList<T>`)
            if (SerializationUtil.IsTableForgeSerializable(TypeMatchMode.GenericArgument, fieldType, out cellType))
                return CreateCellInstance(cellType, column, row, fieldInfo);

            // Handle special cases
            if (fieldType.IsEnum)
                return new EnumCell(column, row, fieldInfo);

            if (!fieldType.IsSimpleType())
                return new SubItemCell(column, row, fieldInfo);

            Debug.LogWarning($"Unsupported type: {fieldType}\nField: {fieldInfo?.Name}\nAt table: {column.Table.Name}\nPosition: {column.LetterPosition}{row.Position}");
            return new DefaultCell(column, row, fieldInfo);
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
        /// <returns>An instance of the specified Cell type, or null if creation fails.</returns>
        private static Cell CreateCellInstance(Type cellType, Column column, Row row, TfFieldInfo fieldInfo)
        {
            if (_cellConstructors.TryGetValue(cellType, out var constructor))
                return (Cell)constructor.Invoke(new object[] { column, row, fieldInfo });
            
            constructor = cellType.GetConstructor(
                new[] { typeof(Column), typeof(Row), typeof(TfFieldInfo)}
            );

            if (constructor != null)
            {
                _cellConstructors.TryAdd(cellType, constructor);
                return (Cell)constructor.Invoke(new object[] { column, row, fieldInfo });
            }

            Debug.LogError($"{cellType.Name} lacks required constructor.");
            return null;
        }

        #endregion
    }
}
