using System;

namespace TableForge.Attributes
{
    /// <summary>
    /// Specifies that a field or property should be ignored during serialization by TableForge.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a field or property to prevent it from being serialized  
    /// when TableForge processes the object.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ExampleClass
    /// {
    ///     public int IncludedField { get; set; }
    ///     
    ///     [TableForgeIgnore]
    ///     public string IgnoredField { get; set; }
    /// }
    /// </code>
    /// In this example, `IncludedField` will be serialized, but `IgnoredField` will not.
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TableForgeIgnoreAttribute : Attribute
    {
    }
}