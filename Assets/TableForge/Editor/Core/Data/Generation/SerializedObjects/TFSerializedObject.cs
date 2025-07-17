using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    /// <summary>
    /// Default implementation of ITFSerializedObject. Represents a single object in a table.
    /// </summary>
    internal class TfSerializedObject : ITfSerializedObject
    {
        protected IColumnGenerator columnGenerator;
        
        public Object RootObject { get; }
        public string RootObjectGuid { get; }
        public string Name { get; protected set; }
        public object TargetInstance { get; protected set; }
        public TfSerializedType SerializedType { get; protected set; }
        

        public TfSerializedObject(object targetInstance, FieldInfo parentField, Object rootObject, string guid, string name = null, Type typeOverride = null)
        {
            TargetInstance = targetInstance;
            SerializedType = new TfSerializedType(typeOverride ?? targetInstance.GetType(), parentField);
            columnGenerator = SerializedType;
            RootObject = rootObject;
            RootObjectGuid = guid;
            
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
            if(!SerializedType.Fields.Contains(cell.fieldInfo))
                throw new ArgumentException($"Field {cell.fieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.fieldInfo.GetValue(TargetInstance);
        }

        public virtual void SetValue(Cell cell, object data)
        {
            if(!SerializedType.Fields.Contains(cell.fieldInfo))
                throw new ArgumentException($"Field {cell.fieldInfo.Name} is not a valid field for this object!");
            
            if(TargetInstance == null)
                return;
            
            Cell parentCell = cell.row.Table.ParentCell;
            if (parentCell != null && SerializedType.IsStruct && parentCell is SubTableCell parentSubTableCell and not ICollectionCell)
            {
                /*As we are dealing always with type "object", value types are boxed in reference types,
                 so TargetInstance is in fact a reference to the boxed value type. This means that we need to
                 create a copy of the struct, to be able to change the value of the field in the struct without
                 affecting the original value that could be stored in other SerializedObjects as a reference.  
                 
                 Doing this we can mimic the behaviour of a real value type even when we are dealing with reference types internally.               
                 */
                
                object structInstance = TargetInstance;
                object copy = TargetInstance.CreateShallowCopy();

                //We change the value inside TargetInstance without affecting the original value that could be stored in other SerializedObjects as a reference.
                cell.fieldInfo.SetValue(copy, data);
                TargetInstance = structInstance; //We restore the original value of TargetInstance
                
                //Now it's time to update the parent cell with the new value
                parentSubTableCell.SetValue(copy);
                TargetInstance = copy; //We update the value of TargetInstance with the new value reference
            }
            else cell.fieldInfo.SetValue(TargetInstance, data);
            
            if(!EditorUtility.IsDirty(RootObject))
                EditorUtility.SetDirty(RootObject);
        }

        public virtual Type GetValueType(Cell cell)
        {
            if(!SerializedType.Fields.Contains(cell.fieldInfo))
                throw new ArgumentException($"Field {cell.fieldInfo.Name} is not a valid field for this object!");
            
            return TargetInstance == null ? null : cell.fieldInfo.Type;
        }
        
        public virtual void PopulateRow(List<Column> columns, Table table, Row row)
        {
            columnGenerator.GenerateColumns(columns, table);
            
            for (var j = 0; j < SerializedType.Fields.Count; j++)
            {
                Cell cell = CellFactory.CreateCell(columns[j], row, SerializedType.Fields[j].Type, SerializedType.Fields[j]);
                row.AddCell(j + 1, cell);
            }
        }
    }
}
