using System;
using System.Collections.Generic;
using System.Reflection;
using TableForge.Editor.Serialization;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for complex types that are serialized as subtables.
    /// </summary>
    internal class SubItemCell : SubTableCell
    {
        public SubItemCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Serializer = new SubItemCellSerializer(this);
            
            if (cachedValue == null)
            {
                if (fieldInfo == null)
                {
                    CreateSubTable();
                    return;
                }

                if (((Cell)this).fieldInfo?.FieldInfo.GetCustomAttribute<SerializeReference>() == null)
                {
                    try
                    {
                        cachedValue = fieldInfo.Type.CreateInstanceWithDefaults();
                        SetFieldValue(cachedValue);
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

        protected sealed override void CreateSubTable()
        {
            if (cachedValue == null)
            {
                SubTable = TableGenerator.GenerateTable(new TfSerializedType(Type, fieldInfo?.FieldInfo),
                    $"{Table.Name}.{column.Name}", this);
                return;
            }

            ITfSerializedObject serializedObject = new TfSerializedObject(cachedValue, fieldInfo?.FieldInfo,
                TfSerializedObject.RootObject, fieldInfo?.FriendlyName ?? cachedValue.GetType().Name);

            if (SubTable != null)
                TableGenerator.GenerateTable(SubTable, new List<ITfSerializedObject> { serializedObject });
            else
                SubTable = TableGenerator.GenerateTable(new List<ITfSerializedObject> { serializedObject },
                    $"{column.Table.Name}.{serializedObject.Name}", this);
        }

        public void CreateDefaultValue()
        {
            if (cachedValue != null)
                return;

            if (fieldInfo == null)
            {
                Debug.LogWarning("FieldInfo is null, cannot create default value.");
                return;
            }

            try
            {
                SetValue(fieldInfo.Type.CreateInstanceWithDefaults());
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"Failed to create instance of {fieldInfo.Type} for {fieldInfo.FriendlyName} in table {column.Table.Name}, row {row.Position}.\n{e.Message}");
            }
        }

        public override int CompareTo(Cell other)
        {
            if (other is not SubItemCell otherSubItemCell)
                return 1;

            if (cachedValue == null && otherSubItemCell.cachedValue == null) return 0;
            if (cachedValue == null) return -1;
            if (otherSubItemCell.cachedValue == null) return 1;

            return 0;
        }
    }
}