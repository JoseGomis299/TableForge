namespace TableForge
{
    /// <summary>
    /// Cell for boolean type fields.
    /// </summary>
    [CellType(typeof(bool))]
    internal class BoolCell : Cell
    {
        public BoolCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject) : base(column, row, fieldInfo, tfSerializedObject) { }

        public override void SerializeData()
        {
        
        }
    }
}