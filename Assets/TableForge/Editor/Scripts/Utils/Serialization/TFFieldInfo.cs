using System;
using System.Reflection;

namespace TableForge
{
    /// <summary>
    /// Represents metadata for a serializable field in TableForge.
    /// Provides methods to get and set field values dynamically using reflection.
    /// </summary>
    internal class TFFieldInfo
    {
        #region Fields

        private readonly FieldInfo _fieldInfo;

        #endregion
        
        #region Properties

        /// <summary>
        /// The internal name of the field.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// A user-friendly name for display.
        /// </summary>
        public string FriendlyName { get; }
        
        /// <summary>
        /// The type that declares the field.
        /// </summary>
        public Type FromType { get; }
        
        /// <summary>
        /// The data type of the field.
        /// </summary>
        public Type Type { get; }

        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of <see cref="TFFieldInfo"/>.
        /// </summary>
        /// <param name="name">The internal name of the field.</param>
        /// <param name="friendlyName">A user-friendly name for display.</param>
        /// <param name="fromType">The type that declares the field.</param>
        /// <param name="type">The data type of the field.</param>
        public TFFieldInfo(string name, string friendlyName, Type fromType, Type type)
        {
            Name = name;
            FriendlyName = friendlyName;
            FromType = fromType;
            Type = type;

            _fieldInfo = FromType.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_fieldInfo == null)
                throw new ArgumentException($"Field '{Name}' not found in type '{FromType.FullName}'.");
        }

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Gets the value of the field from a given target object.
        /// </summary>
        /// <param name="target">The object instance from which to retrieve the field value.</param>
        /// <returns>The field value, or null if not found.</returns>
        public virtual object GetValue(object target)
        {
            return _fieldInfo?.GetValue(target);
        }

        /// <summary>
        /// Sets the value of the field for a given target object.
        /// </summary>
        /// <param name="target">The object instance to modify.</param>
        /// <param name="value">The new value to assign to the field.</param>
        public virtual void SetValue(object target, object value)
        {
            _fieldInfo?.SetValue(target, value);
        }
        
        #endregion
    }
}
