using System.Linq;
using UnityEngine;

namespace TableForge.Editor
{
    /// <summary>
    /// Cell for Unity LayerMask type fields.
    /// </summary>
    [CellType(typeof(LayerMask))]
    internal class LayerMaskCell : Cell, IQuotedValueCell
    {
        public LayerMaskCell(Column column, Row row, TfFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
        
        public override string Serialize()
        {
            return ((LayerMask)GetValue()).ResolveName();
        }
        
        public string SerializeQuotedValue()
        { 
            return "\"" + Serialize() + "\"";
        }

        public override void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            
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
        
        public override int CompareTo(Cell other)
        {
            if (other is not LayerMaskCell) return 1;
            
            LayerMask thisMask = (LayerMask)GetValue();
            LayerMask otherMask = (LayerMask)other.GetValue();
            
            // Compare the value of the masks
            return thisMask.value.CompareTo(otherMask.value);
        }
    }
}