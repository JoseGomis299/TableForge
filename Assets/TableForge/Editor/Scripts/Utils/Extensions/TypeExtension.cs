using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TableForge
{
    internal static class TypeExtension
    {
        #region Fields

        /// <summary>
        /// A set of integral types for quick lookup.
        /// </summary>
        public static readonly HashSet<Type> IntegralTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong)
        };

        /// <summary>
        /// A set of floating-point types for quick lookup.
        /// </summary>
        public static readonly HashSet<Type> FloatingPointTypes = new HashSet<Type>
        {
            typeof(float), typeof(double)
        };

        #endregion

        #region Type Classification Methods

        /// <summary>
        /// Determines whether the specified type is a simple type (primitive, string, decimal, or enum).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is simple; otherwise, false.</returns>
        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive 
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type.IsEnum;
        }

        /// <summary>
        /// Determines whether the specified type is a serializable class or struct.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is serializable and not a Unity object, primitive, or string.</returns>
        public static bool IsSerializableClassOrStruct(this Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type) || 
                type.IsPrimitive || 
                type == typeof(string)) 
                return false;

            return type.IsDefined(typeof(SerializableAttribute)) && 
                   (type.IsClass || type.IsValueType);
        }

        /// <summary>
        /// Determines whether the specified type is an integral type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an integral type; otherwise, false.</returns>
        public static bool IsIntegralType(this Type type)
        {
            return IntegralTypes.Contains(type);
        }

        /// <summary>
        /// Determines whether the specified type is a floating-point type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a floating-point type; otherwise, false.</returns>
        public static bool IsFloatingPointType(this Type type)
        {
            return FloatingPointTypes.Contains(type);
        }

        #endregion

        #region Collection Type Methods

        /// <summary>
        /// Determines whether the specified type is a list or array.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a list or array; otherwise, false.</returns>
        public static bool IsListOrArrayType(this Type type)
        {
            // Handle arrays
            if (type.IsArray)
            {
                return true;
            }

            // Handle generic lists
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return true;
            }

            // Handle non-generic lists
            return type.GetInterface(nameof(IList)) != null;
        }

        #endregion

        #region Reflection Methods

        /// <summary>
        /// Retrieves the member information of a given property path in a type.
        /// </summary>
        /// <param name="type">The type to search in.</param>
        /// <param name="path">The property path.</param>
        /// <returns>The matching <see cref="MemberInfo"/> if found, otherwise null.</returns>
        public static MemberInfo GetMemberViaPath(this Type type, string path)
        {
            var parentType = type;
            path = path.Split('.')[0];

            // First check fields with backing field names
            MemberInfo memberInfo = parentType.GetField($"<{path}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            // Then check regular fields
            memberInfo ??= parentType.GetField(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Then check properties
            memberInfo ??= parentType.GetProperty(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            return memberInfo;
        }
        
        

        #endregion
    }
}
