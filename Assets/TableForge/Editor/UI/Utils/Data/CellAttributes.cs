using System;

namespace TableForge.UI
{
    /// <summary>
    /// Specifies the type of cell that is bound to a UI element representing a cell and how the cell size will be calculated.
    /// </summary>
    public struct CellAttributes
    {
        /// <summary>
        /// Gets the type of the mapped cell.
        /// </summary>
        public Type CellType { get; }
        
        /// <summary>
        /// The method used to calculate the size of the cell.
        /// </summary>
        public CellSizeCalculationMethod SizeCalculationMethod { get; }
        
        public CellAttributes(Type cellType, CellSizeCalculationMethod sizeCalculationMethod)
        {
            CellType = cellType;
            SizeCalculationMethod = sizeCalculationMethod;
        }
    }
}