using System.Linq;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Unity LayerMask type fields.
    /// </summary>
    [CellType(typeof(LayerMask))]
    internal class LayerMaskCell : Cell
    {
        public LayerMaskCell(Column column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            return $"\'{((LayerMask)GetValue()).ResolveName()}\'";
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
            data = data.Trim('\'');

            if(data == "Everything")
            {
                SetValue(new LayerMask {value = int.MaxValue});
                return;
            }
            
            if(data == "Nothing")
            {
                SetValue(new LayerMask {value = 0});
                return;
            }
            
            LayerMask value = LayerMask.GetMask(data.Split(",").Select(x => x.Trim()).ToArray());
            SetValue(value);
        }
    }
}