using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for complex types that are serialized as subtables.
    /// </summary>
    internal class SubItemCell : SubTableCell
    {
        public SubItemCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
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

        protected sealed override void CreateSubTable()
        {
            if (Value == null)
            {
                SubTable = TableGenerator.GenerateTable(new TFSerializedType(Type, FieldInfo?.FieldInfo), $"{Table.Name}.{Column.Name}", this);
                return;
            }
            
            ITFSerializedObject serializedObject = new TFSerializedObject(Value, FieldInfo?.FieldInfo, TfSerializedObject.RootObject, FieldInfo?.FriendlyName ?? Value.GetType().Name);
            
            if(SubTable != null)
                TableGenerator.GenerateTable(SubTable, new List<ITFSerializedObject>{serializedObject});
            else 
                SubTable = TableGenerator.GenerateTable(new List<ITFSerializedObject>{serializedObject}, $"{Column.Table.Name}.{serializedObject.Name}", this);
        }

        public void CreateDefaultValue()
        {
            if (Value != null)
                return;
            
            if (FieldInfo == null)
            {
                Debug.LogWarning("FieldInfo is null, cannot create default value.");
                return;
            }

            try
            {
                SetValue(FieldInfo.Type.CreateInstanceWithDefaults());
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"Failed to create instance of {FieldInfo.Type} for {FieldInfo.FriendlyName} in table {Column.Table.Name}, row {Row.Position}.\n{e.Message}");
            }
        }

        public override int CompareTo(Cell other)
        {
            if (other is not SubItemCell otherSubItemCell)
                return 1;

            if (Value == null && otherSubItemCell.Value == null) return 0;
            if (Value == null) return -1;
            if (otherSubItemCell.Value == null) return 1;

            return 0;
        }

        protected override void DeserializeModifyingSubTable(string[]values, ref int index)
        {
            if(Value != null && values[0].Equals(SerializationConstants.EmptyColumn))
            {
                SetValue(null);
                return;
            }
            
            if(Value == null && !values[0].Equals(SerializationConstants.EmptyColumn))
            {
                CreateDefaultValue();
            }
            
            DeserializeSubItem(values, ref index);
        }

        protected override void DeserializeWithoutModifyingSubTable(string[]values, ref int index)
        {
            DeserializeSubItem(values, ref index);
        }

        private void DeserializeSubItem(string[] values, ref int index)
        {
            foreach (var descendant in this.GetImmediateDescendants())
            {
                if (index >= values.Length)
                {
                    if(SerializationConstants.ModifySubTables)
                        break;
                    index = 0;
                }
                
                DeserializeCell(values, ref index, descendant);
            }
        }
    }
}