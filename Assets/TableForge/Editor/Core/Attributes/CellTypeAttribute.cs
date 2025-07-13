using System;

namespace TableForge.Editor
{
    /// <summary>
    /// Defines how type matching should be performed when applying the <see cref="CellTypeAttribute"/>.
    /// </summary>
    internal enum TypeMatchMode
    {
        /// <summary>
        /// Matches only the exact type.
        /// <example>
        /// `string` will match only `string`, but not `object` or `StringBuilder`.
        /// </example>
        /// </summary>
        Exact,
        
        /// <summary>
        /// Matches any type that is assignable to the specified type.
        /// <example>
        /// `IList` will match `List<int>`, `List<string>`, and any class implementing `IList`.
        /// </example>
        /// </summary>
        Assignable,
        
        /// <summary>
        /// Matches based on generic arguments.
        /// <example>
        /// `List<>` will match `List<int>`, `List<string>`, etc.
        /// </example>
        /// </summary>
        GenericArgument
    }

    /// <summary>
    /// An attribute used to specify which types a cell class supports.  
    /// It can be applied multiple times to support different type match modes.
    /// </summary>
    /// <remarks>
    /// This attribute is only effective when applied to a **non-abstract** class that  
    /// inherits from <see cref="Cell"/>. Otherwise, it has no effect.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class CellTypeAttribute : Attribute
    {
        #region Properties
        
        public Type[] SupportedTypes { get; }
        public TypeMatchMode MatchMode { get; }
        
        #endregion

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CellTypeAttribute"/> class  
        /// with an exact type match mode.
        /// </summary>
        /// <param name="supportedTypes">The types this attribute applies to.</param>
        /// <example>
        /// <code>
        /// [CellType(typeof(int), typeof(float))]
        /// internal class NumberCell : Cell { }
        /// </code>
        /// </example>
        public CellTypeAttribute(params Type[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
            MatchMode = TypeMatchMode.Exact;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CellTypeAttribute"/> class  
        /// with a specified match mode.
        /// </summary>
        /// <param name="matchMode">The type matching mode.</param>
        /// <param name="supportedTypes">The types this attribute applies to.</param>
        /// <example>
        /// <code>
        /// [CellType(TypeMatchMode.Assignable, typeof(IEnumerable))]
        /// internal class CollectionCell : Cell { }
        /// </code>
        /// </example>
        public CellTypeAttribute(TypeMatchMode matchMode, params Type[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
            MatchMode = matchMode;
        }
        
        #endregion
    }
}
