using System.Collections.Generic;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Vector2 type fields. Represented as a subtable with 2 columns (x, y).
    /// </summary>
    [CellType(typeof(Vector2))]
    internal class Vector2Cell : SubTableCell
    {
        public Vector2Cell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo)
        {
            Type = typeof(Vector2);
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
            SubTable = TableGenerator.GenerateTable(itemData, $"{Table.Name}.{Column.Name}", this);
        }
    }
}