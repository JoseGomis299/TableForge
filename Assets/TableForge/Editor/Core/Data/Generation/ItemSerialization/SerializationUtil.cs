using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TableForge.Attributes;
using UnityEngine;
using ICollection = System.Collections.ICollection;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    /// <summary>
    /// Utility class for handling serialization logic in TableForge.
    /// Provides methods for discovering cell types, analyzing field serializability, and managing type mappings.
    /// </summary>
    internal static class SerializationUtil
    {
        #region Private Fields

        /// <summary>
        /// List of Unity types that are automatically serialized by Unity.
        /// </summary>
        private static readonly List<Type> _unitySerializedTypes = new()
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Rect),
            typeof(Bounds),
            typeof(AnimationCurve),
            typeof(Gradient),
            typeof(RectOffset),
            typeof(LayerMask)
        };
        
        /// <summary>
        /// Mapping of type match modes to supported cell types.
        /// </summary>
        private static readonly Dictionary<TypeMatchMode, List<(HashSet<Type> SupportedTypes, Type cellType)>> _cellMappings = new();
        
        /// <summary>
        /// Mapping of type match modes to their corresponding strategies.
        /// </summary>
        private static readonly Dictionary<TypeMatchMode, ICellMappingStrategy> _strategies = new()
        {
            { TypeMatchMode.Exact, new ExactMatchStrategy() },
            { TypeMatchMode.Assignable, new AssignableMatchStrategy() },
            { TypeMatchMode.GenericArgument, new GenericMatchStrategy() }
        };

        #endregion

        #region Static Constructor

        /// <summary>
        /// Static constructor that initializes the cell type mappings.
        /// </summary>
        static SerializationUtil()
        {
            DiscoverCellTypes();
        }
        
        #endregion

        #region Public Methods - Field Analysis

        /// <summary>
        /// Retrieves a list of serializable fields for a given type.
        /// Fields are considered serializable based on Unity's serialization rules and custom attributes.
        /// </summary>
        /// <param name="type">The type to analyze.</param>
        /// <param name="fromField">The field containing the type.</param>
        /// <returns>A list of <see cref="TfFieldInfo"/> representing serializable fields.</returns>
        public static List<TfFieldInfo> GetSerializableFields(Type type, FieldInfo fromField)
        {
            bool isSerializedReference = fromField != null && fromField.GetCustomAttribute<SerializeReference>() != null;
            if (fromField != null && !isSerializedReference && fromField.FieldType != type)
            {
                type = fromField.FieldType;
            }

            return TfFieldInfoFactory.GetFields(type);
        }
        
        /// <summary>
        /// Gets a user-friendly name for a field, handling backing fields for auto-properties.
        /// </summary>
        /// <param name="field">The field to get the friendly name for.</param>
        /// <returns>The friendly name of the field.</returns>
        public static string GetFriendlyName(FieldInfo field)
        {
            if (IsBackingField(field, out var propertyName))
                return propertyName.ConvertToProperCase();
            
            return field.Name.ConvertToProperCase();
        }

        /// <summary>
        /// Determines whether a given field should be considered serializable.
        /// </summary>
        /// <param name="field">The field to analyze.</param>
        /// <param name="allowPrivate">Allow private fields to be serialized.
        /// (This is done in very few cases where unity serializes a field even if it is private and not marked as serializable,
        /// which is the case for the fields in <see cref="Rect"/> and <see cref="Bounds"/>)</param>
        /// <returns>True if the field is serializable; otherwise, false.</returns>
        public static bool IsSerializable(FieldInfo field, bool allowPrivate = false)
        {
            if(field.GetCustomAttribute<TableForgeIgnoreAttribute>() != null || !IsUnitySerializable(field.FieldType))
                return false;
            
            bool isSerializable = field.GetCustomAttribute<SerializeField>() != null || field.IsPublic || allowPrivate || field.GetCustomAttribute<SerializeReference>() != null;
            isSerializable &= !field.Attributes.HasFlag(FieldAttributes.Static) && !field.Attributes.HasFlag(FieldAttributes.InitOnly);
            
            return isSerializable && IsTableForgeSerializable(field.FieldType);
        }

        #endregion

        #region Public Methods - Type Serializability

        /// <summary>
        /// Checks if a type is considered serializable in TableForge.
        /// Supports primitive types, enums, and custom class/struct serialization.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is serializable, otherwise false.</returns>
        public static bool IsTableForgeSerializable(Type type)
        {
            return (IsTableForgeSerializable(TypeMatchMode.Exact, type, out _)
                   || IsTableForgeSerializable(TypeMatchMode.Assignable, type, out _)
                   || IsTableForgeSerializable(TypeMatchMode.GenericArgument, type, out _)
                   || type.IsEnum
                   || !type.IsSimpleType())
                   && type != typeof(object);
        }

        /// <summary>
        /// Determines whether a type is serializable by TableForge based on a specified matching mode.
        /// </summary>
        /// <param name="matchMode">The matching mode to use.</param>
        /// <param name="type">The type to check.</param>
        /// <param name="cellType">The resolved cell type if a match is found.</param>
        /// <returns>True if a valid cell type is found, otherwise false.</returns>
        public static bool IsTableForgeSerializable(TypeMatchMode matchMode, Type type, out Type cellType)
        {
            if (_strategies.TryGetValue(matchMode, out var strategy))
            {
                return strategy.TryGetCellType(type, _cellMappings, out cellType);
            }

            cellType = null;
            return false;
        }

        /// <summary>
        /// Detects circular dependencies between a type and its parent type to prevent infinite recursion.
        /// </summary>
        /// <param name="type">The type being analyzed.</param>
        /// <param name="parentType">The parent type in the serialization hierarchy.</param>
        /// <returns>True if a circular dependency is detected, otherwise false.</returns>
        public static bool HasCircularDependency(Type type, Type parentType)
        {
            if (CheckCircularDependency(type)) 
                return true;

            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                return type.GetGenericArguments().Any(CheckCircularDependency);
            }

            if (type.IsArray)
                return CheckCircularDependency(type.GetElementType());

            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                       .Any(field => CheckCircularDependency(field.FieldType));

            bool CheckCircularDependency(Type t) =>
                t != null && parentType != null && !t.IsSimpleType() && !typeof(Object).IsAssignableFrom(t) &&
                t.Namespace != "UnityEngine" && (parentType.IsAssignableFrom(t) || t.IsAssignableFrom(parentType));
        }

        #endregion

        #region Private Methods - Cell Type Discovery

        /// <summary>
        /// Discovers and registers all available cell types based on attributes in the current assembly.
        /// </summary>
        private static void DiscoverCellTypes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            foreach (var type in assembly.GetTypes())
            {
                if (!typeof(Cell).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                foreach (var attr in type.GetCustomAttributes<CellTypeAttribute>())
                {
                    if (!_cellMappings.TryGetValue(attr.MatchMode, out var mappings))
                    {
                        mappings = new List<(HashSet<Type>, Type)>();
                        _cellMappings[attr.MatchMode] = mappings;
                    }
                    mappings.Add((attr.SupportedTypes.ToHashSet(), type));
                }
            }
        }

        #endregion

        #region Private Methods - Field Analysis

        /// <summary>
        /// Determines if a given field is a compiler-generated backing field for an auto-property.
        /// </summary>
        /// <param name="field">The field to analyze.</param>
        /// <param name="propertyName">The extracted property name if it is a backing field.</param>
        /// <returns>True if the field is a backing field; otherwise, false.</returns>
        private static bool IsBackingField(FieldInfo field, out string propertyName)
        {
            propertyName = null;
            if (!field.Name.StartsWith("<") || !field.Name.EndsWith(">k__BackingField"))
                return false;

            propertyName = field.Name.Substring(1, field.Name.Length - 17);
            return true;
        }

        #endregion

        #region Private Methods - Unity Serialization

        /// <summary>
        /// Determines whether a type is considered serializable by Unity.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is serializable in Unity; otherwise, false.</returns>
        private static bool IsUnitySerializable(Type type)
        {
            if (type == null || type.IsAbstract || type.IsInterface)
                return false;

            if (type.IsArray && type.GetArrayRank() == 1)
                return IsUnitySerializable(type.GetElementType());

            if ((type.IsGenericType && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition())) || typeof(IList).IsAssignableFrom(type))
            {
                Type elementType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
                if (elementType != null && (typeof(IList).IsAssignableFrom(elementType) || elementType.IsArray ||
                                            (elementType.IsGenericType && typeof(IList<>).IsAssignableFrom(elementType.GetGenericTypeDefinition()))))
                    return false;

                return IsUnitySerializable(elementType);
            }
            
            if (type.ImplementsInterface(typeof(ICollection))
                || type.ImplementsInterface(typeof(ICollection<>))
               )
                return typeof(ISerializationCallbackReceiver).IsAssignableFrom(type);

            return type.IsPrimitive || type == typeof(string) || typeof(Object).IsAssignableFrom(type) ||
                   type.IsEnum || type.GetCustomAttribute<SerializableAttribute>() != null ||
                   typeof(ISerializationCallbackReceiver).IsAssignableFrom(type) || _unitySerializedTypes.Contains(type);
        }

        #endregion
    }
}
