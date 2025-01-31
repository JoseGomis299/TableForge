using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Default implementation of ITFSerializedObject. Represents a single object in a table.
    /// </summary>
    internal class TFSerializedObject : ITFSerializedObject
    {
        public string Name { get; protected set; }
        
        protected List<TFFieldInfo> Fields;
        protected object TargetInstance;
        
        protected TFSerializedObject() { }
        
        public TFSerializedObject(object targetInstance)
        {
            TargetInstance = targetInstance;
            Fields = SerializationUtil.GetSerializableFields(targetInstance.GetType());

            if (targetInstance is Object unityObject)
            {
                Name = unityObject.name;
                return;
            }
            
            Name = targetInstance.GetType().Name;
        }
        
        public TFSerializedObject(string name, object targetInstance)
        {
            TargetInstance = targetInstance;
            Name = name;
            Fields = SerializationUtil.GetSerializableFields(targetInstance.GetType());
        }        

        public virtual object GetValue(Cell cell)
        {
            if(!Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.FieldInfo.GetValue(TargetInstance);
        }

        public virtual void SetValue(Cell cell, object data)
        {
            if(!Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            if(TargetInstance == null)
                return;
            
            cell.FieldInfo.SetValue(TargetInstance, data);
        }

        public virtual Type GetValueType(Cell cell)
        {
            if(!Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.FieldInfo.Type;
        }
        
        public virtual void PopulateRow(List<CellAnchor> columns, Table table, Row row)
        {
            for (var j = 0; j < Fields.Count; j++)
            {
                var member = Fields[j];
                if (columns.Count < Fields.Count)
                {
                    columns.Add(new CellAnchor(member.FriendlyName, columns.Count + 1));
                    table.AddColumn(columns[j]);
                }

                Cell cell = CellFactory.CreateCell(columns[j], row, Fields[j].Type, Fields[j], this);
                row.Cells.Add(j + 1, cell);
            }
        }
    }
}
