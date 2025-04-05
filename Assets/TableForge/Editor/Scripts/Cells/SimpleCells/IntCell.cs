using TableForge.Exceptions;

namespace TableForge
{
    /// <summary>
    /// Cell for integer values.
    /// </summary>
    [CellType(/*typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), */typeof(int), typeof(uint), typeof(long), typeof(ulong))]
    internal class IntCell : Cell
    {
        public IntCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    }

    // [CellType(typeof(byte))]
    // internal class ByteCell : Cell
    // {
    //     public ByteCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    // }
    //
    // [CellType(typeof(sbyte))]
    // internal class SByteCell : Cell
    // {
    //     public SByteCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    // }
    //
    // [CellType(typeof(short))]
    // internal class ShortCell : Cell
    // {
    //     public ShortCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    // }
    //
    // [CellType(typeof(ushort))]
    // internal class UShortCell : Cell
    // {
    //     public UShortCell(CellAnchor column, Row row, TFFieldInfo fieldInfo) : base(column, row, fieldInfo) { }
    // }
    
    
}