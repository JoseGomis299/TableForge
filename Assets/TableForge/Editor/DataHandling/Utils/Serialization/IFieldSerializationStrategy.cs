using System;
using System.Collections.Generic;
using System.Reflection;

namespace TableForge.Editor
{
    internal interface IFieldSerializationStrategy
    {
        List<TfFieldInfo> GetFields(Type type);
    }

    internal class BaseFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TfFieldInfo> GetFields(Type type)
        {
            List<TfFieldInfo> members = new List<TfFieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!SerializationUtil.IsSerializable(field)) continue;
                if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;
                
                string friendlyName = SerializationUtil.GetFriendlyName(field);
                
                members.Add(new TfFieldInfo(field.Name, friendlyName, type, field.FieldType));
            }

            return members;
        }
    }

    internal class PrivateIncludedFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TfFieldInfo> GetFields(Type type)
        {
            List<TfFieldInfo> members = new List<TfFieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!SerializationUtil.IsSerializable(field, true)) continue;
                if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;
                
                string friendlyName = SerializationUtil.GetFriendlyName(field);
                
                members.Add(new TfFieldInfo(field.Name, friendlyName, type, field.FieldType));
            }

            return members;
        }
    }
}