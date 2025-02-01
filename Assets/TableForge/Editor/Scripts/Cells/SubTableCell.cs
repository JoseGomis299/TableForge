namespace TableForge
{
    /// <summary>
    /// Represents a cell that contains a subtable, typically used for complex or collection-based fields.
    /// </summary>
    internal abstract class SubTableCell : Cell
    {
        #region Properties

        /// <summary>
        /// Indicates whether the subtable is in an invalid state. This happens when the collection has been modified outside TableForge.
        /// </summary>
        public bool IsSubTableInvalid { get; set; }

        /// <summary>
        /// The subtable associated with this cell.
        /// </summary>
        public Table SubTable { get; protected set; }

        #endregion

        #region Constructors
        protected SubTableCell(CellAnchor column, Row row, TFFieldInfo fieldInfo, ITFSerializedObject tfSerializedObject)
            : base(column, row, fieldInfo, tfSerializedObject) { }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates and initializes the subtable associated with this cell.
        /// </summary>
        protected abstract void CreateSubTable();

        #endregion
    }
}