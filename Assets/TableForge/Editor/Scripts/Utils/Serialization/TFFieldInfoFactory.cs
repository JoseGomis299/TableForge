using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    internal static class TFFieldInfoFactory
    {
        private static Dictionary<Type, List<TFFieldInfo>> _fieldCache = new Dictionary<Type, List<TFFieldInfo>>();
        
        public static List<TFFieldInfo> GetFields(Type type)
        {
            if (_fieldCache.TryGetValue(type, out var cachedFields))
                return cachedFields;
            
            IFieldSerializationStrategy strategy = GetStrategy(type);
            List<TFFieldInfo> fields = strategy.GetFields(type);
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