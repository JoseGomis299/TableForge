namespace TableForge.Editor.UI
{
    public enum CellSizeCalculationMethod
    {
        /// <summary>
        /// Fixed size for big cells.
        /// </summary>
        FixedBigCell,
        /// <summary>
        /// Fixed size for regular cells.
        /// </summary>
        FixedRegularCell,
        /// <summary>
        /// Fixed size for small cells.
        /// </summary>
        FixedSmallCell,
        /// <summary>
        /// Calculates size based on the content of the cell.
        /// </summary>
        AutoSize,
        /// <summary>
        /// Calculates size based on the content of the cell, adding the necessary space for enum fields.
        /// </summary>
        EnumAutoSize,
        /// <summary>
        /// Calculates size based on the content of the cell, adding the necessary space for reference fields.
        /// </summary>
        ReferenceAutoSize
    }
}