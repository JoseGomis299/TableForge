using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for complex types that are serialized as subtables.
    /// </summary>
    internal class SubItemCell : SubTableCell
    {
        public SubItemCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            if (Value == null)
            {
                if (fieldInfo == null)
                {
                    CreateSubTable();
                    return;
                }

                if (FieldInfo?.FieldInfo.GetCustomAttribute<SerializeReference>() == null)
                {
                    try
                    {
                        Value = fieldInfo.Type.CreateInstanceWithDefaults();
                        SetFieldValue(Value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(
                            $"Failed to create instance of {fieldInfo.Type} for {fieldInfo.FriendlyName} in table {column.Table.Name}, row {row.Position}.\n{e.Message}");
                        CreateSubTable();
                        return;
                    }
                }
            }
            
            CreateSubTable();
        }

        public override void SetValue(object value)
        {
            base.SetValue(value);
            CreateSubTable();
        }
        
        public override void RefreshData()
        {
        
        }

        protected sealed override void CreateSubTable()
        {
            if (Value == null)
            {
                SubTable = TableGenerator.GenerateTable(new TFSerializedType(Type, FieldInfo?.FieldInfo), $"{Table.Name}.{Column.Name}", this);
                return;
            }
            
            ITFSerializedObject serializedObject = new TFSerializedObject(Value, FieldInfo?.FieldInfo, FieldInfo?.FriendlyName ?? Value.GetType().Name);
            
            SubTable = TableGenerator.GenerateTable(new List<ITFSerializedObject>{serializedObject}, $"{Column.Table.Name}.{serializedObject.Name}", this);
        }
    }
}