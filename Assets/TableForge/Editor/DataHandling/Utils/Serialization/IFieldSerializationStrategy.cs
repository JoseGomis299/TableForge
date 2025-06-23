using System;
using System.Collections.Generic;
using System.Reflection;
using TableForge.Editor;

namespace TableForge
{
    internal interface IFieldSerializationStrategy
    {
        List<TFFieldInfo> GetFields(Type type);
    }

    internal class BaseFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TFFieldInfo> GetFields(Type type)
        {
            List<TFFieldInfo> members = new List<TFFieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!SerializationUtil.IsSerializable(field)) continue;
                if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;
                
                string friendlyName = SerializationUtil.GetFriendlyName(field);
                
                members.Add(new TFFieldInfo(field.Name, friendlyName, type, field.FieldType));
            }

            return members;
        }
    }

    internal class PrivateIncludedFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TFFieldInfo> GetFields(Type type)
        {
            List<TFFieldInfo> members = new List<TFFieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!SerializationUtil.IsSerializable(field, true)) continue;
                if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;
                
                string friendlyName = SerializationUtil.GetFriendlyName(field);
                
                members.Add(new TFFieldInfo(field.Name, friendlyName, type, field.FieldType));
            }

            return members;
        }
    }
}