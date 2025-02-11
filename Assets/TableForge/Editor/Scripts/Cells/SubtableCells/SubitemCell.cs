using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for complex types that are serialized as subtables.
    /// </summary>
    internal class SubitemCell : SubTableCell
    {
        public SubitemCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject)
        {
            Type = typeof(ITFSerializedObject);
            object value = GetFieldValue();
            
            if (value == null)
            {
                if(fieldInfo == null) return;
                
                try
                {
                    value = fieldInfo.Type.CreateInstanceWithDefaults();
                    SetFieldValue(value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to create instance of {fieldInfo.Type} for {fieldInfo.FriendlyName} in table {column.Table.Name}, row {row.Position}.\n{e.Message}");
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
        
        public override void SerializeData()
        {
        
        }

        protected sealed override void CreateSubTable()
        {
            SubTable = TableGenerator.GenerateTable(new List<ITFSerializedObject>{(TFSerializedObject)Value}, $"{Column.Table.Name}.{((TFSerializedObject)Value).Name}", this);
        }
    }
}