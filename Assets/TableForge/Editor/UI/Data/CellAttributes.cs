using System;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Specifies the type of cell that is bound to a UI element representing a cell and how the cell size will be calculated.
    /// </summary>
    public struct CellAttributes
    {
        public Type CellType { get; }
        
        public CellSizeCalculationMethod SizeCalculationMethod { get; }
        
        public CellAttributes(Type cellType, CellSizeCalculationMethod sizeCalculationMethod)
        {
            CellType = cellType;
            SizeCalculationMethod = sizeCalculationMethod;
        }
    }
}