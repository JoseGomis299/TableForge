using System;
using System.Collections.Generic;
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
            Type = typeof(ITFSerializedObject);
            object value = GetFieldValue();
            
            if (value == null)
            {
                if (fieldInfo == null)
                {
                    CreateSubTable();
                    return;
                }
                
                try
                {
                    value = fieldInfo.Type.CreateInstanceWithDefaults();
                    SetFieldValue(value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to create instance of {fieldInfo.Type} for {fieldInfo.FriendlyName} in table {column.Table.Name}, row {row.Position}.\n{e.Message}");
                    CreateSubTable();
                    return;
                }
            }
            
            Value = new TFSerializedObject(fieldInfo?.FriendlyName ?? value.GetType().Name, value);
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
                SubTable = TableGenerator.GenerateTable(new TFSerializedType(Type), $"{Column.Table.Name}.{Column.Name}", this);
                return;
            }
            
            SubTable = TableGenerator.GenerateTable(new List<ITFSerializedObject>{(TFSerializedObject)Value}, $"{Column.Table.Name}.{((TFSerializedObject)Value).Name}", this);
        }
    }
}