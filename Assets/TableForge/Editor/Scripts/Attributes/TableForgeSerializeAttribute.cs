using System;

namespace TableForge
{
    /// <summary>
    /// Marks a field or property to be explicitly serialized by TableForge.
    /// </summary>
    /// <remarks>
    /// By default, Unity automatically serializes certain fields and properties.  
    /// This attribute is used to explicitly mark fields or properties that Unity does not serialize,  
    /// but should be included in TableForge serialization.  
    /// <para>
    /// To exclude a field or property from serialization, use <see cref="TableForgeIgnoreAttribute"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ExampleClass
    /// {
    ///     [TableForgeSerialize]
    ///     private string customSerializedField;
    ///     
    ///     [TableForgeIgnore]
    ///     public int ignoredField;
    /// }
    /// </code>
    /// In this example, `customSerializedField` will be serialized by TableForge,  
    /// while `ignoredField` will be excluded.
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TableForgeSerializeAttribute : Attribute
    {
    }
}