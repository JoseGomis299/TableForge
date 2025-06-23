using System;
using System.Collections.Generic;
using System.Reflection;

namespace TableForge.Editor
{
    /// <summary>
    /// Represents the metadata of a serialized type compatible with TableForge.
    /// </summary>
    internal class TfSerializedType : IColumnGenerator
    {
        private readonly List<TfFieldInfo> _fields;
        public IReadOnlyList<TfFieldInfo> Fields => _fields;
        public bool IsStruct {get;}
        
        public Type Type { get; }
        
        public TfSerializedType(Type type, FieldInfo parentField)
        {
            _fields = SerializationUtil.GetSerializableFields(type, parentField);
            IsStruct = type.IsValueType;
            Type = type;
        }

        public void GenerateColumns(List<Column> columns, Table table)
        {
            if(columns.Count == _fields.Count) return;
            
            for (var j = 0; j < _fields.Count; j++)
            {
                var member = _fields[j];
                if (columns.Count < _fields.Count)
                {
                    columns.Add(new Column(member.FriendlyName, columns.Count + 1, table));
                }
            }
        }
    }
}