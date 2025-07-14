using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.Editor
{
    internal static class TfFieldInfoFactory
    {
        private static readonly Dictionary<Type, List<TfFieldInfo>> _fieldCache = new();
        
        public static List<TfFieldInfo> GetFields(Type type)
        {
            if (_fieldCache.TryGetValue(type, out var cachedFields))
                return cachedFields;
            
            IFieldSerializationStrategy strategy = GetStrategy(type);
            List<TfFieldInfo> fields = strategy.GetFields(type);
            _fieldCache.Add(type, fields);
            return fields;
        }
        
        private static IFieldSerializationStrategy GetStrategy(Type type)
        {
            if(type == typeof(Rect) || type == typeof(Bounds)) 
                return new PrivateIncludedFieldSerializationStrategy();
            
            return new BaseFieldSerializationStrategy();
        }
    }
}