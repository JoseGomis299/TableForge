using System;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity Gradient type fields.
    /// </summary>
    [CellType(typeof(Gradient))]
    internal class GradientCell : Cell
    {
        public GradientCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }

        public override void SetValue(object value)
        {
            base.SetValue(value);
            if(Value != null) return;

            Value = new Gradient();
        }

        public override void RefreshData()
        {
            base.RefreshData();
            if(Value != null) return;

            Value = new Gradient();
        }

        public override string Serialize()
        {
            SerializableGradient data = new SerializableGradient((Gradient) GetValue());
            return Serializer.Serialize(data);
        }
        
        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            SerializableGradient value = Serializer.Deserialize<SerializableGradient>(data);
            if (value is not null)
            {
                SetValue(value.ToGradient());
            }
        }
        
        public override int CompareTo(Cell otherCell)
        {
            if (otherCell is not GradientCell) return 1;

            Gradient thisGradient = (Gradient)GetValue();
            Gradient otherGradient = (Gradient)otherCell.GetValue();

            // Compare the number of color keys
            return thisGradient.colorKeys.Length.CompareTo(otherGradient.colorKeys.Length);
        }
    }
}