using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace TableForge
{
    /// <summary>
    /// Cell for Enum fields.
    /// </summary>
    internal class EnumCell : Cell
    {
        public EnumCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }

        public override void SerializeData()
        {
        
        }
    }
}