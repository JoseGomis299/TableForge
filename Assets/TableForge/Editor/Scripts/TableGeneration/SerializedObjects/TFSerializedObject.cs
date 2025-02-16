using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Default implementation of ITFSerializedObject. Represents a single object in a table.
    /// </summary>
    internal class TFSerializedObject : ITFSerializedObject
    {
        public string Name { get; protected set; }
        
        protected TFSerializedType SerializedType;
        protected IColumnGenerator ColumnGenerator;
        protected object TargetInstance;
        
        protected TFSerializedObject() { }
        
        public TFSerializedObject(object targetInstance)
        {
            TargetInstance = targetInstance;
            SerializedType = new TFSerializedType(targetInstance.GetType());
            ColumnGenerator = SerializedType;
            
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
            SerializedType = new TFSerializedType(targetInstance.GetType());
            ColumnGenerator = SerializedType;
        }        

        public virtual object GetValue(Cell cell)
        {
            if(!SerializedType.Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.FieldInfo.GetValue(TargetInstance);
        }

        public virtual void SetValue(Cell cell, object data)
        {
            if(!SerializedType.Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            if(TargetInstance == null)
                return;
            
            cell.FieldInfo.SetValue(TargetInstance, data);
        }

        public virtual Type GetValueType(Cell cell)
        {
            if(!SerializedType.Fields.Contains(cell.FieldInfo))
                throw new ArgumentException($"Field {cell.FieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.FieldInfo.Type;
        }
        
        public virtual void PopulateRow(List<CellAnchor> columns, Table table, Row row)
        {
            ColumnGenerator.GenerateColumns(columns, table);
            row.SerializedObject = this;
            
            for (var j = 0; j < SerializedType.Fields.Count; j++)
            {
                Cell cell = CellFactory.CreateCell(columns[j], row, SerializedType.Fields[j].Type, SerializedType.Fields[j]);
                row.Cells.Add(j + 1, cell);
            }
        }
    }
}
