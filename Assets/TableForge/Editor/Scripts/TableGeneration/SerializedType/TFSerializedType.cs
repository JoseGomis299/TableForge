using System;
using System.Collections.Generic;
using System.Reflection;

namespace TableForge
{
    /// <summary>
    /// Represents the metadata of a serialized type compatible with TableForge.
    /// </summary>
    internal class TFSerializedType : IColumnGenerator
    {
        private readonly List<TFFieldInfo> _fields;
        public IReadOnlyList<TFFieldInfo> Fields => _fields;
        public bool IsStruct {get;}
        
        public Type Type { get; }
        
        public TFSerializedType(Type type, FieldInfo parentField)
        {
            _fields = SerializationUtil.GetSerializableFields(type, parentField);
            IsStruct = type.IsValueType;
            Type = type;
        }

        public void GenerateColumns(List<CellAnchor> columns, Table table)
        {
            if(columns.Count == _fields.Count) return;
            
            for (var j = 0; j < _fields.Count; j++)
            {
                var member = _fields[j];
                if (columns.Count < _fields.Count)
                {
                    columns.Add(new CellAnchor(member.FriendlyName, columns.Count + 1));
                    table.AddColumn(columns[j]);
                }
            }
        }
    }
}