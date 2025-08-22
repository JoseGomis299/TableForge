using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TableForge.Editor
{
    internal interface IFieldSerializationStrategy
    {
        List<TfFieldInfo> GetFields(Type type);
    }

    internal class BaseFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TfFieldInfo> GetFields(Type baseType)
        {
            HashSet<string> fields = new HashSet<string>();
            List<TfFieldInfo> members = new List<TfFieldInfo>();

            foreach (var type in baseType.GetBaseTypes(breakType: typeof(ScriptableObject)))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fields.Contains(field.Name)) continue;
                    if (!SerializationUtil.IsSerializable(field)) continue;
                    if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;

                    string friendlyName = SerializationUtil.GetFriendlyName(field);

                    members.Add(new TfFieldInfo(field.Name, friendlyName, type, field.FieldType));
                    fields.Add(field.Name); // Add field to the set to avoid duplicates
                }
            }

            return members;
        }
    }

    internal class PrivateIncludedFieldSerializationStrategy : IFieldSerializationStrategy
    {
        public List<TfFieldInfo> GetFields(Type baseType)
        {
            HashSet<string> fields = new HashSet<string>();
            List<TfFieldInfo> members = new List<TfFieldInfo>();
            
            foreach (var type in baseType.GetBaseTypes(breakType: typeof(ScriptableObject)))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fields.Contains(field.Name)) continue;
                    if (!SerializationUtil.IsSerializable(field, true)) continue;
                    if (SerializationUtil.HasCircularDependency(field.FieldType, type)) continue;

                    string friendlyName = SerializationUtil.GetFriendlyName(field);

                    members.Add(new TfFieldInfo(field.Name, friendlyName, type, field.FieldType));
                    fields.Add(field.Name); // Add field to the set to avoid duplicates
                }
            }

            return members;
        }
    }
}