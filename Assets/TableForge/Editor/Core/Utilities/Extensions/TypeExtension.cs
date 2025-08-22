using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TableForge.Editor
{
    /// <summary>
    /// Provides extension methods for Type analysis and manipulation in TableForge.
    /// Includes type classification, reflection utilities, and enum handling.
    /// </summary>
    internal static class TypeExtension
    {
        #region Fields

        /// <summary>
        /// A set of integral types for quick lookup.
        /// </summary>
        public static readonly HashSet<Type> IntegralTypes = new()
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong)
        };

        /// <summary>
        /// A set of floating-point types for quick lookup.
        /// </summary>
        public static readonly HashSet<Type> FloatingPointTypes = new()
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
                   || type.IsEnum
                   || typeof(UnityEngine.Object).IsAssignableFrom(type);
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
        /// Retrieves all base types of a given type, including the type itself.
        /// </summary>
        public static List<Type> GetBaseTypes(this Type type, bool includeSelf = true, Type breakType = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), "Type cannot be null.");

            var baseTypes = new List<Type>();
            if (includeSelf) baseTypes.Add(type);
            
            Type current = type.BaseType;
            while (current != null)
            {
                if (breakType != null && current == breakType)
                    break;

                baseTypes.Insert(0, current);
                current = current.BaseType;
            }

            return baseTypes;
        }
       
        
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

        /// <summary>
        /// Creates an instance of the specified type using the most suitable public constructor.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>The created default instance.</returns>
        /// <exception cref="InvalidOperationException">If the type has no valid constructors.</exception>
        public static object CreateInstanceWithDefaults(this Type type)
        {
            if(type.IsValueType)
                return Activator.CreateInstance(type);
            
            // Get all constructors and sort by parameter count and create an instance with default values
            ConstructorInfo[] constructors = type.GetConstructors();
            var sortedConstructors = constructors.OrderBy(ctor => ctor.GetParameters().Length);
            
            foreach (var constructor in sortedConstructors)
            {
                try
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    object[] defaultValues = parameters
                        .Select(param =>
                            param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null)
                        .ToArray();

                    return constructor.Invoke(defaultValues);
                }
                catch
                {
                    continue;
                }
            }

            // If no suitable constructor is found or all invocations fail, throw an exception
            throw new InvalidOperationException($"No suitable constructor found for type {type.FullName}.");
        }
        
        /// <summary>
        /// Checks if a type implements a specific interface.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="interfaceType">The interface to check if is implemented.</param>
        /// <returns>True if the type implements the interface, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Some of the given types are null.</exception>
        /// <exception cref="ArgumentException">The interface type is not an interface.</exception>
        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            if (type == null || interfaceType == null)
                throw new ArgumentNullException("Type parameters cannot be null.");

            if (!interfaceType.IsInterface)
                throw new ArgumentException("The provided interfaceType must be an interface.");

            return type.GetInterfaces().Any(it =>
                it.IsGenericType
                    ? it.GetGenericTypeDefinition() == interfaceType
                    : it == interfaceType);
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Resolves a flagged enum value to a human-readable string representation.
        /// </summary>
        /// <param name="enumType">The enum type to resolve.</param>
        /// <param name="value">The enum value to resolve.</param>
        /// <param name="makePretty">Whether to convert the enum names to proper case.</param>
        /// <returns>A string representation of the flagged enum value.</returns>
        public static string ResolveFlaggedEnumName(this Type enumType, int value, bool makePretty = true)
        {
            if (value == -1)
            {
                return "Everything";
            }

            if (value == 0)
            {
                if(Enum.IsDefined(enumType, 0))
                {
                    return Enum.GetName(enumType, 0);
                }
                return "Nothing";
            }

            string res = string.Empty;
            foreach (var name in Enum.GetNames(enumType))
            {
                int enumValue = (int)Enum.Parse(enumType, name);
                if ((value & enumValue) != 0)
                {
                    if (!string.IsNullOrEmpty(res))
                    {
                        res += ", ";
                    }
                    if (makePretty)
                        res += name.ConvertToProperCase();
                    else
                        res += name;
                }
            }

            return res;
        }

        #endregion
    }
}
