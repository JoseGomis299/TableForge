using System;
using System.Collections;
using System.Reflection;

namespace TableForge.Editor
{
    public static class ObjectExtension
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

            if (obj is IDictionary dictionary)
            {
                var copy = Activator.CreateInstance(type) as IDictionary;
                foreach (var key in dictionary.Keys)
                {
                    var keyCopy = key.CreateShallowCopy();
                    var valueCopy = dictionary[key].CreateShallowCopy();
                    copy.Add(keyCopy, valueCopy);
                }
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
        
        
        /// <summary>
        /// Parses an object to a double value.
        /// </summary>
        public static bool TryParseNumber(this object value, out double result)
        {
            result = 0;
            if (value == null) 
                return false;

            if (value is string s) value = s.Replace('.', ',');
            return double.TryParse(value.ToString(), out result);
        }
        
        /// <summary>
        /// Parses an object to a boolean value.
        /// </summary>
        public static bool TryParseBoolean(this object value, out bool result)
        {
            result = false;
            if (value is bool boolValue)
            {
                result = boolValue;
                return true;
            }

            if (value is string strValue && bool.TryParse(strValue, out bool parsedValue))
            {
                result = parsedValue;
                return true;
            }

            return false;
        }
    }
}