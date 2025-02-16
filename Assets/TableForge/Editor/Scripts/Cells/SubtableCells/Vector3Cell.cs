using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Vector3 type fields. Represented as a subtable with 3 columns (x, y, z).
    /// </summary>
    [CellType(typeof(Vector3))]
    internal class Vector3Cell : SubTableCell
    {
        public Vector3Cell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Type = typeof(Vector3);
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
            itemData.Add(new TFSerializedObject(Value));
            SubTable = TableGenerator.GenerateTable(itemData, $"{Column.Table.Name}.{Column.Name}", this);
        }
    }
}