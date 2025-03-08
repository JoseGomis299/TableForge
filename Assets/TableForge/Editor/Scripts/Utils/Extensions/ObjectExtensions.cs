using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TableForge
{
    public static class ObjectExtensions
    {
        
        /// <summary>
        ///  Creates a shallow copy of the object.
        /// </summary>
        /// <param name="obj">The object to copy.</param>
        /// <returns></returns>
        public static object CreateShallowCopy(this object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();
            if (type.IsSimpleType() || type == typeof(string))
                return obj;

            if (type.IsArray)
            {
                var array = obj as Array;
                var copy = array.Clone() as Array;
                for (var i = 0; i < array.Length; i++)
                    copy.SetValue(array.GetValue(i).CreateShallowCopy(), i);
                return copy;
            }

            if (type.IsClass || type.IsValueType)
            {
                var copy = type.CreateInstanceWithDefaults();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;

                    field.SetValue(copy, fieldValue.CreateShallowCopy());
                }

                return copy;
            }

            return null;
        }
    }
}