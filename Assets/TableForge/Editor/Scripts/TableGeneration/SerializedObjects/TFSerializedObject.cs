using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TableForge
{
    /// <summary>
    /// Default implementation of ITFSerializedObject. Represents a single object in a table.
    /// </summary>
    internal class TFSerializedObject : ITFSerializedObject
    {
        protected TFSerializedType SerializedType;
        protected IColumnGenerator ColumnGenerator;
        
        public string Name { get; protected set; }
        public object TargetInstance { get; protected set; }
        
        protected TFSerializedObject() { }

        public TFSerializedObject(object targetInstance, FieldInfo parentField, string name = null)
        {
            TargetInstance = targetInstance;
            SerializedType = new TFSerializedType(targetInstance.GetType(), parentField);
            ColumnGenerator = SerializedType;

            if (name == null)
            {
                if (targetInstance is Object unityObject)
                    Name = unityObject.name;
                else
                    Name = targetInstance.GetType().Name;
            }
            else Name = name;
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
            
            Cell parentCell = cell.Row.Table.ParentCell;
            if (parentCell != null && SerializedType.IsStruct && parentCell is SubTableCell parentSubTableCell and not ICollectionCell)
            {
                object structInstance = TargetInstance;
                MethodInfo memberwiseClone = structInstance.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                object copy = memberwiseClone.Invoke(structInstance, null);
                
                cell.FieldInfo.SetValue(copy, data);
                TargetInstance = structInstance;
                parentSubTableCell.SetValue(copy);
                TargetInstance = copy;
            }
            else cell.FieldInfo.SetValue(TargetInstance, data);
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
