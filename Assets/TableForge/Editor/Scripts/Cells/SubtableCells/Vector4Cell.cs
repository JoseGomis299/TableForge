using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Vector4 type fields. Represented as a subtable with 4 columns (x, y, z, w).
    /// </summary>
    [CellType(typeof(Vector4))]
    internal class Vector4Cell : SubTableCell
    {
        public Vector4Cell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Type = typeof(Vector4);
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
            List<ITFSerializedObject> itemData = new List<ITFSerializedObject>();
            itemData.Add(new TFSerializedObject(Value, FieldInfo?.FieldInfo));
            SubTable = TableGenerator.GenerateTable(itemData, $"{Column.Table.Name}.{Column.Name}", this);
        }
    }
}