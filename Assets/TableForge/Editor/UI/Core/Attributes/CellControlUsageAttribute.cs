using System;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// An attribute used to specify the type of cell that is bound to a UI element representing a cell and how the cell size will be calculated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class CellControlUsageAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// The type of cell that is bound to a UI element representing a cell and how the cell size will be calculated.
        /// </summary>
        public CellAttributes CellAttributes { get; }
        
        #endregion

        #region Constructors

        /// <summary>
        ///  Initializes a new instance of the <see cref="CellControlUsageAttribute"/> class.
        /// </summary>
        /// <param name="cellType">The type of cell mapped to the ui element.</param>
        /// <param name="sizeCalculationMethod">How this cell size will be calculated</param>
        /// <exception cref="ArgumentException">If the given type doesn't inherit from Cell.</exception>
        public CellControlUsageAttribute(Type cellType, CellSizeCalculationMethod sizeCalculationMethod)
        {
            if(!typeof(Cell).IsAssignableFrom(cellType))
                throw new ArgumentException("The provided type must inherit from Cell.", nameof(cellType));
         
            CellAttributes = new CellAttributes(cellType, sizeCalculationMethod);
        }
        #endregion
    }
}